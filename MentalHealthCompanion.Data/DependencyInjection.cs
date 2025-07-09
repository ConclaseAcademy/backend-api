using MentalHealthCompanion.Data.DataContext;
using MentalHealthCompanion.Data.Interface;
using MentalHealthCompanion.Data.Options;
using MentalHealthCompanion.Data.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MentalHealthCompanion.Data
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAuthenService, AuthenService>();
            services.AddScoped<IJwtService, JwtService>();
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.Section));
            return services;
        }

        public static async Task SeedDatabaseAsync(this WebApplication app)
        {
            await DbSeeder.SeedAsync(app);
        }
    }
}
