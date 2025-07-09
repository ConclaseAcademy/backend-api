using MentalHealthCompanion.Data.DTO;
using MentalHealthCompanion.Data.DTO.General;
using MentalHealthCompanion.Data.DTO.RequestDto;
using MentalHealthCompanion.Data.DTO.ResponseDto;

namespace MentalHealthCompanion.Data.Interface
{
    public interface IAuthenService
    {
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto loginRequestDto, CancellationToken cancellationToken);

        Task<ApiResponse<UserRegistrationResponseDto>> RegisterAsync(UserRegistrationRequestDto model, RegistrationRequestDto registrationRequest);

        Task<ApiResponse<UserRegistrationResponseDto>> RegisterAdminUserAsync(
            CreateAdminRequestDto createdAdminRequestDto, CancellationToken token);

        Task<ApiResponse<AuthResponseDto>> SetAdminNewPasswordAsync(
            ResetAdminPasswordDto resetAdminPasswordDto, string emailAdress, CancellationToken token);
    }
}
