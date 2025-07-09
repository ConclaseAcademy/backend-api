using MentalHealthCompanion.Data.DTO.RequestDto;

namespace MentalHealthCompanion.Data.Interface
{
    public interface IEmailNotificationService
    {
        Task<bool> SendEmailAsync(EmailRequestDto model);
    }
}
