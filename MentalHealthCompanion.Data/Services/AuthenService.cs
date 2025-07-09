using MentalHealthCompanion.Data.AppResponses;
using MentalHealthCompanion.Data.DatabaseEntities;
using MentalHealthCompanion.Data.DataContext;
using MentalHealthCompanion.Data.DTO;
using MentalHealthCompanion.Data.DTO.General;
using MentalHealthCompanion.Data.DTO.RequestDto;
using MentalHealthCompanion.Data.DTO.ResponseDto;
using MentalHealthCompanion.Data.Enums;
using MentalHealthCompanion.Data.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace MentalHealthCompanion.Data.Services
{
    public class AuthenService : IAuthenService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IJwtService _jwtService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AuthenService> _logger;
        private readonly IEmailNotificationService _emailNotificationService;

        public AuthenService(AppDbContext appDbContext, IJwtService jwtService, UserManager<IdentityUser> userManager, ILogger<AuthenService> logger, IEmailNotificationService emailNotificationService)
        {
            _appDbContext = appDbContext;
            _jwtService = jwtService;
            _userManager = userManager;
            _logger = logger;
            _emailNotificationService = emailNotificationService;
        }
        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto loginRequestDto, CancellationToken cancellationToken)
        {
            var existingUser = await _appDbContext
                .AppUsers
                .FirstOrDefaultAsync(u => u.EmailAddress == loginRequestDto.Email, cancellationToken);

            var userManagerResult = await _userManager.FindByIdAsync(existingUser?.Id!);
            if (existingUser is null || userManagerResult is null)
            {
                return new ApiResponse<AuthResponseDto>(ResponseCodes.UserNotFound, ResponseMessages.UserNotFound);
            }

            var token = _jwtService.GenerateToken(existingUser, cancellationToken);
            var passwordCheck = await _userManager.CheckPasswordAsync(userManagerResult, loginRequestDto.Password);
            if (!passwordCheck)
            {
                return new ApiResponse<AuthResponseDto>(ResponseCodes.InvalidRegistrationCode, "Invalid Password");
            }

            if (existingUser.Role.Equals(UserRole.Admin.ToString()) && !existingUser.IsAdminPasswordChanged)
            {
                return new ApiResponse<AuthResponseDto>()
                {
                    Data = new AuthResponseDto
                    {
                        Token = token,
                        Role = existingUser.Role
                    },
                    Code = ResponseCodes.Success,
                    Message = "Kindly Change your password to continue",
                    IsSuccessful = true,
                    Timestamp = DateTime.UtcNow
                };
            }

            return new ApiResponse<AuthResponseDto>()
            {
                Data = new AuthResponseDto
                {
                    Token = token,
                    Role = existingUser.Role
                },
                Code = ResponseCodes.Success,
                Message = ResponseMessages.Success,
                IsSuccessful = true,
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<ApiResponse<UserRegistrationResponseDto>> RegisterAsync(UserRegistrationRequestDto model, RegistrationRequestDto registrationRequest)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            int saveChanges = 0;

            using var transaction = await _appDbContext.Database.BeginTransactionAsync();

            try
            {
                //validate email address
                var existingUser = await _userManager.FindByEmailAsync(model?.EmailAddress!);

                if (existingUser != null)
                {
                    _logger.LogInformation("Email address {@email} already used", model?.EmailAddress);
                    return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.EmailAlreadyExist, ResponseMessages.UserExist);
                }

                //create identity user record
                IdentityUser identityUser = new()
                {
                    Email = model.EmailAddress,
                    UserName = model.EmailAddress
                };

                var addIdentityUser = await _userManager.CreateAsync(identityUser);

                if (addIdentityUser.Succeeded == false)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError("Registration of user with email address {@email} failed", model?.EmailAddress);
                    return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.UserRegistrationFailed, ResponseMessages.UserRegistrationFailed);
                }
                
                // creating the user's password
                var result = await _userManager.AddPasswordAsync(identityUser, model.Password);
                if (result.Succeeded is false)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError("Registration of user with email address {@email} failed", model?.EmailAddress);
                    return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.UserRegistrationFailed, ResponseMessages.InvalidPassword);
                }

                //create app user record
                AppUser appUser = new()
                {
                    Id = identityUser.Id,
                    EmailAddress = model.EmailAddress!,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = registrationRequest.UserRole,
                };

                var addUser = await _appDbContext.AppUsers.AddAsync(appUser);
                saveChanges = await _appDbContext.SaveChangesAsync();

                if (saveChanges < 1)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError("Registration of user with email address {@email} failed", model?.EmailAddress);
                    return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.UserRegistrationFailed, ResponseMessages.UserRegistrationFailed);
                }

                //assign role
                var addRoleResponse = await _userManager.AddToRoleAsync(identityUser, registrationRequest.UserRole);

                if (addRoleResponse.Succeeded == false)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError("Registration of user with email address {@email} failed", model?.EmailAddress);
                    return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.UserRegistrationFailed, ResponseMessages.UserRegistrationFailed);
                }

                await transaction.CommitAsync();

                //todo: send email to the user with the link to complete registration
                await _emailNotificationService.SendEmailAsync(new EmailRequestDto()
                {
                    RecipientEmail = model.EmailAddress,
                    SenderEmail = "noreply@mentalhealthmanagement.com",
                    Subject = "User Onboarding",
                    Message = "Registration Successful"
                });

                var responseData = new UserRegistrationResponseDto()
                {
                    Message = "Registration Successful."
                };

                _logger.LogInformation("User registration successful");
                return new ApiResponse<UserRegistrationResponseDto>(new(), ResponseCodes.Success, ResponseMessages.UserRegistrationSuccessful);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
                await transaction.RollbackAsync();
                return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.Exception, ResponseMessages.Exception);
            }
        }

        public async Task<ApiResponse<UserRegistrationResponseDto>> RegisterAdminUserAsync(CreateAdminRequestDto createdAdminRequestDto, CancellationToken token)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                int saveChanges = 0;
                using var transaction = await _appDbContext.Database.BeginTransactionAsync(token);
                IdentityUser? identityUser = null;
                string passwordGenerated = string.Empty;
                AppUser? appUser = null;
                try
                {
                    var existingIdentityUser = await _userManager.FindByEmailAsync(createdAdminRequestDto.Email!);

                    if (existingIdentityUser is null)
                    {
                        // password generated
                        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_-+=<>?";
                        passwordGenerated = new string(Enumerable.Range(0, 6)
                            .Select(_ => validChars[RandomNumberGenerator.GetInt32(validChars.Length)]).ToArray());

                        _logger.LogInformation("user password generated {0}", passwordGenerated);
                        _logger.LogInformation("No user with such email {U} hence creating new user ", createdAdminRequestDto.Email);
                        _logger.LogInformation("Creating user with email {U}", createdAdminRequestDto.Email);

                        int random = RandomNumberGenerator.GetInt32(0, 200);

                        identityUser = new IdentityUser
                        {
                            Email = createdAdminRequestDto.Email,
                            UserName = $"Admin{random}",
                        };

                        appUser = new()
                        {
                            Id = identityUser.Id,
                            EmailAddress = createdAdminRequestDto.Email,
                            Role = UserRole.Admin.ToString()
                        };
                        var createUserResult = await _userManager.CreateAsync(identityUser, passwordGenerated);

                        if (createUserResult.Succeeded == false)
                        {
                            var errorList = createUserResult.Errors.ToList();
                            await transaction.RollbackAsync();
                            _logger.LogError("Registration of user with email address {@email} failed", createdAdminRequestDto?.Email);

                            return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.ProfileUpdateFailed, ResponseMessages.UserRegistrationFailed);
                        }
                        var addUser = await _appDbContext.AppUsers.AddAsync(appUser);
                        saveChanges = await _appDbContext.SaveChangesAsync();

                        if (saveChanges < 1)
                        {
                            await transaction.RollbackAsync(token);
                            _logger.LogError("Registration of user with email address {@email} failed", createdAdminRequestDto?.Email);
                            return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.UserRegistrationFailed, ResponseMessages.UserRegistrationFailed);
                        }
                    }
                    else
                    {

                        var existingAppUser = await _appDbContext.AppUsers.FirstOrDefaultAsync(u => u.EmailAddress == createdAdminRequestDto.Email);

                        if (existingAppUser is null)
                        {
                            _logger.LogInformation("User does not exists {u}", createdAdminRequestDto.Email);
                            return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.UserNotFound, ResponseMessages.UserNotFound);
                        }

                        // Updating the existing user
                        existingAppUser.Role = UserRole.Admin.ToString();
                        _appDbContext.AppUsers.Update(existingAppUser);
                        saveChanges = await _appDbContext.SaveChangesAsync();

                        if (saveChanges < 1)
                        {
                            await transaction.RollbackAsync(token);
                            _logger.LogError("Registration of user with email address {@email} failed", createdAdminRequestDto?.Email);
                            return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.UserRegistrationFailed, ResponseMessages.UserRegistrationFailed);
                        }

                        identityUser = await _userManager.FindByEmailAsync(createdAdminRequestDto.Email);

                        if (identityUser is null)
                        {
                            _logger.LogError("User with email address {@email} does not exist", createdAdminRequestDto?.Email);
                            return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.UserNotFound, ResponseMessages.UserNotFound);
                        }

                        await _userManager.RemoveFromRoleAsync(identityUser, UserRole.RegularUser.ToString());
                        var addRoleResponse = await _userManager.AddToRoleAsync(identityUser, UserRole.Admin.ToString());

                        if (addRoleResponse.Succeeded == false)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogError("Failed adding Admin role: {role}", UserRole.Admin.ToString());
                            return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.UserRegistrationFailed, ResponseMessages.UserRegistrationFailed);
                        }
                    }

                    await transaction.CommitAsync(token);

                    //todo: send email to the user with the link to complete registration
                    bool isSent = await _emailNotificationService.SendEmailAsync(new EmailRequestDto()
                    {
                        RecipientEmail = createdAdminRequestDto.Email,
                        SenderEmail = "noreply@hospitalmanagement.com",
                        Subject = $"Admin Password change",
                        Message = $"Please find your admin password: {passwordGenerated}"
                    });
                    if (!isSent)
                    {
                        _logger.LogError("Failed to send email to user with email address {@email}", createdAdminRequestDto?.Email);
                        return new ApiResponse<UserRegistrationResponseDto>("unable to send admin password to User", ResponseMessages.Exception);
                    }
                    var responseData = new UserRegistrationResponseDto()
                    {
                        Message = "The link to complete registration has been sent to the email address."
                    };

                    _logger.LogInformation("User registration successful");
                    return new ApiResponse<UserRegistrationResponseDto>(responseData, ResponseCodes.Success, ResponseMessages.UserRegistrationSuccessful);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError($"{ex}");
                    await transaction.RollbackAsync();
                    return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.EmailAlreadyExist, ResponseMessages.UpdateException);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{ex}");
                    await transaction.RollbackAsync();
                    return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.Exception, ResponseMessages.Exception);
                }
            });
        }

        public async Task<ApiResponse<AuthResponseDto>> SetAdminNewPasswordAsync(
            ResetAdminPasswordDto resetAdminPasswordDto, string email, CancellationToken token)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _appDbContext.Database.BeginTransactionAsync(token);

                try
                {
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        return new ApiResponse<AuthResponseDto>(ResponseCodes.ValiationError, ResponseMessages.Exception);
                    }
                    var existingAdminUser = await _appDbContext.AppUsers.FirstOrDefaultAsync(u => u.EmailAddress == email);
                    var userIdentity = await _userManager.FindByEmailAsync(email);
                    if (existingAdminUser is null || userIdentity is null)
                    {
                        return new ApiResponse<AuthResponseDto>(ResponseCodes.UserNotFound, ResponseMessages.UserNotFound);
                    }

                    var isValidPassword = await _userManager.CheckPasswordAsync(userIdentity, resetAdminPasswordDto.SentPassword);

                    if (!isValidPassword)
                        return new ApiResponse<AuthResponseDto>(ResponseCodes.ValiationError, ResponseMessages.Exception);

                    // setting the admin-Password
                    existingAdminUser.IsAdminPasswordChanged = true;

                    var saveChangesResult = await _appDbContext.SaveChangesAsync();
                    if (saveChangesResult < 1)
                    {
                        await transaction.RollbackAsync(token);
                        return new ApiResponse<AuthResponseDto>(ResponseCodes.ValiationError, ResponseMessages.Exception);
                    }

                    var passwordChangedResult = await _userManager.ChangePasswordAsync(userIdentity, resetAdminPasswordDto.SentPassword, resetAdminPasswordDto.NewPassword);

                    if (!passwordChangedResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError("Password change failed: {@errors}", passwordChangedResult.Errors);
                        return new ApiResponse<AuthResponseDto>(ResponseCodes.UserRegistrationFailed, "Failed to change password");
                    }

                    await transaction.CommitAsync(token);

                    return new ApiResponse<AuthResponseDto>(new AuthResponseDto(), ResponseCodes.Success, "Password Changed Successfully");

                }
                catch (Exception ex)
                {
                    _logger.LogError($"{ex}");
                    await transaction.RollbackAsync();
                    return new ApiResponse<AuthResponseDto>(ResponseCodes.Exception, ResponseMessages.Exception);
                }
            });

        }
    }
}
