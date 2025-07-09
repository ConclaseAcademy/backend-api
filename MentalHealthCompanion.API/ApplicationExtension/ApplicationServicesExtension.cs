using MentalHealthCompanion.Data.DataContext;
using MentalHealthCompanion.Data.Interface;
using MentalHealthCompanion.Data.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Text;

namespace MentalHealthCompanion.API.ApplicationExtension
{
    public static class ApplicationServicesExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            #region Database Context
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(config.GetConnectionString("Database"));

            });
            #endregion

            #region Services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IEmailNotificationService, EmailNotificationService>();
            services.AddScoped<IJwtService, JwtService>();
            #endregion

            #region Send Grid
            var sender = config["SendGrid:Sender"];
            var from = config["SendGrid:From"];
            var apiKey = config["SendGrid:ApiKey"];

            services
                .AddFluentEmail(sender, from)
                .AddSendGridSender(apiKey);

            services
                .AddSendGrid(options =>
                {
                    options.ApiKey = apiKey;
                });
            #endregion

            #region Authentication
            services.AddAuthorization(option =>
            {
                option.AddPolicy("CreateAdmin", config => config.RequireClaim(ClaimTypes.Role, "SuperAdmin"));
                option.AddPolicy("AdminPassword", config => config.RequireClaim(ClaimTypes.Role, "Admin"));
            });
            #endregion

            return services;
        }
    }
}
