using MentalHealthCompanion.Data.DatabaseEntities;
using MentalHealthCompanion.Data.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MentalHealthCompanion.Data.DataContext
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(this WebApplication app)
        {

            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            var dbContext = services.GetRequiredService<AppDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

            var superAdminRoleName = UserRole.SuperAdmin.ToString();
            var adminRoleName = UserRole.Admin.ToString();
            var regularUserRoleName = UserRole.RegularUser.ToString();
            if (!await roleManager.RoleExistsAsync(regularUserRoleName))
            { 
                await roleManager.CreateAsync(new IdentityRole(regularUserRoleName));
            }
            if (!await roleManager.RoleExistsAsync(superAdminRoleName))
            {
                await roleManager.CreateAsync(new IdentityRole(superAdminRoleName));
            }
            if (!await roleManager.RoleExistsAsync(adminRoleName))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRoleName));
            }

            const string adminEmail = "D.abudu@conclaseint.com";
            const string adminPassword = "Admin@123";

            var existingUser = await userManager.FindByEmailAsync(adminEmail);
            IdentityUser identityUser;
            if (existingUser == null)
            {
                identityUser = new IdentityUser
                {
                    UserName = "superadmin",
                    Email = adminEmail,
                    NormalizedEmail = adminEmail.ToUpper(),
                    EmailConfirmed = true
                };
                var createdResult = await userManager.CreateAsync(identityUser, adminPassword);
                if (!createdResult.Succeeded)
                {
                    throw new Exception("Failed to create admin user: " +
                        string.Join(", ", createdResult.Errors.Select(e => e.Description)));
                }
                await userManager.AddToRoleAsync(identityUser, adminRoleName);
            }
            else
            {
                identityUser = existingUser;
            }

            if (!dbContext.AppUsers.Any(u => u.Id == identityUser.Id))
            {
                var adminUser = new AppUser
                {
                    Id = identityUser.Id.ToString(),
                    FirstName = "SuperAdmin",
                    LastName = "User",
                    EmailAddress = "D.abudu@conclaseint.com",
                    Role = superAdminRoleName
                };
                dbContext.AppUsers.Add(adminUser);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
