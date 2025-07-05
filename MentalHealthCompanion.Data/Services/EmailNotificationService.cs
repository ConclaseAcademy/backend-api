using FluentEmail.Core;
using MentalHealthCompanion.Data.DTO.RequestDto;
using MentalHealthCompanion.Data.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalHealthCompanion.Data.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(ILogger<EmailNotificationService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<bool> SendEmailAsync(EmailRequestDto model)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var mailer = scope.ServiceProvider.GetRequiredService<IFluentEmail>();

                var email = mailer
                    .To(model.RecipientEmail)
                    .Subject(model.Subject)
                    .Tag("juhefjh")
                    .Body(model.Message);

                var sendMailResponse = await email.SendAsync();
                return sendMailResponse.Successful;
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred, {@ex}", ex);
                return false;
            }
        }
    }
}
