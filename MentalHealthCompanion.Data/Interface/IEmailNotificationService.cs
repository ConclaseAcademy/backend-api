using MentalHealthCompanion.Data.DTO.RequestDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalHealthCompanion.Data.Interface
{
    public interface IEmailNotificationService
    {
        Task<bool> SendEmailAsync(EmailRequestDto model);
    }
}
