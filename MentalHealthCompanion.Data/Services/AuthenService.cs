using MentalHealthCompanion.Data.AppResponses;
using MentalHealthCompanion.Data.DatabaseEntities;
using MentalHealthCompanion.Data.DataContext;
using MentalHealthCompanion.Data.DTO.General;
using MentalHealthCompanion.Data.DTO.RequestDto;
using MentalHealthCompanion.Data.DTO.ResponseDto;
using MentalHealthCompanion.Data.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

            var passwordCheck = await _userManager.CheckPasswordAsync(userManagerResult, loginRequestDto.Password);
            if (!passwordCheck)
            {
                return new ApiResponse<AuthResponseDto>(ResponseCodes.ValiationError, ResponseMessages.InvalidPassword);
            }

            var token = _jwtService.GenerateToken(existingUser, cancellationToken);

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
                    Email = model.EmailAddress
                };

                var addIdentityUser = await _userManager.CreateAsync(identityUser);

                if (addIdentityUser.Succeeded == false)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError("Registration of user with email address {@email} failed", model?.EmailAddress);
                    return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.UserRegistrationFailed, ResponseMessages.UserRegistrationFailed);
                }

                //create app user record
                AppUser appUser = new()
                {
                    Id = identityUser.Id,
                    EmailAddress = model.EmailAddress!,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Password = model.Password
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
                return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.Success, ResponseMessages.UserRegistrationSuccessful);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex}");
                await transaction.RollbackAsync();
                return new ApiResponse<UserRegistrationResponseDto>(ResponseCodes.Exception, ResponseMessages.Exception);
            }
        }
    }
}
