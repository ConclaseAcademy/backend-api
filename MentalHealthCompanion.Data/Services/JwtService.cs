using MentalHealthCompanion.Data.DatabaseEntities;
using MentalHealthCompanion.Data.Interface;
using MentalHealthCompanion.Data.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MentalHealthCompanion.Data.Services
{
    public class JwtService : IJwtService
    {
        private readonly ILogger<JwtService> _logger;
        private readonly IOptionsMonitor<JwtOptions> _jwtOptions;
        public JwtService(ILogger<JwtService> logger, IOptionsMonitor<JwtOptions> jwtOptions)
        {
            _logger = logger;
            _jwtOptions = jwtOptions;
        }

        public string GenerateToken(AppUser appUser, CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTime.UtcNow;
                JwtSecurityTokenHandler jwtHandler = new();

                var secretKey = _jwtOptions.CurrentValue.SigningKey;
                var encodedKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                List<Claim> claims = new()
                {
                    new Claim(ClaimTypes.NameIdentifier, appUser.Id.ToString()),
                    new Claim(ClaimTypes.Email, appUser.EmailAddress),
                    new Claim(ClaimTypes.Name, appUser.FirstName ?? "Unknown"),
                    new Claim(ClaimTypes.Role, appUser.Role ?? "User"),
                    new Claim(JwtRegisteredClaimNames.Sub, appUser.EmailAddress),
                    new Claim(JwtRegisteredClaimNames.Jti, new DateTimeOffset(now).ToUnixTimeSeconds().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, now.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, appUser.EmailAddress),
                    new Claim(JwtRegisteredClaimNames.FamilyName, appUser.LastName ?? "Unknown"),
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = now.AddHours(1), 
                    IssuedAt = now,
                    Issuer = _jwtOptions.CurrentValue.Issuer,
                    Audience = _jwtOptions.CurrentValue.Audience,
                    SigningCredentials = new SigningCredentials(encodedKey, SecurityAlgorithms.HmacSha256)
                };
                SecurityToken securityToken = jwtHandler.CreateToken(tokenDescriptor);
                string token = jwtHandler.WriteToken(securityToken);

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user {UserId}", appUser.Id);
                throw;
            }
        }
    }
}
