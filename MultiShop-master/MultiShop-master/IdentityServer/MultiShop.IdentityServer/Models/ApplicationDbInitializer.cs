using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MultiShop.IdentityServer.Models
{
    public static class ApplicationDbInitializer
    {
        public const string AdminRole = "Admin";
        public const string CustomerRole = "Customer";

        public const string DefaultAdminUserName = "admin@gmail.com";
        public const string DefaultAdminEmail = "admin@gmail.com";
        public const string DefaultAdminPassword = "Eren5143.";

        public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { AdminRole, CustomerRole })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminUser = await userManager.FindByNameAsync(DefaultAdminUserName);
            if (adminUser == null)
            {
                adminUser = await userManager.FindByEmailAsync(DefaultAdminEmail);
            }
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = DefaultAdminUserName,
                    Email = DefaultAdminEmail,
                    EmailConfirmed = true,
                    Name = "MultiShop",
                    Surname = "Admin"
                };
                var createResult = await userManager.CreateAsync(adminUser, DefaultAdminPassword);
                if (!createResult.Succeeded)
                {
                    return;
                }
            }
            else
            {
                // Existing user: align login identifiers to the shared admin mailbox.
                if (!string.Equals(adminUser.UserName, DefaultAdminUserName, StringComparison.OrdinalIgnoreCase))
                {
                    adminUser.UserName = DefaultAdminUserName;
                }
                if (!string.Equals(adminUser.Email, DefaultAdminEmail, StringComparison.OrdinalIgnoreCase))
                {
                    adminUser.Email = DefaultAdminEmail;
                }
                adminUser.EmailConfirmed = true;
                await userManager.UpdateAsync(adminUser);
            }

            // Ensure the known admin password works in demo environments.
            var validPassword = await userManager.CheckPasswordAsync(adminUser, DefaultAdminPassword);
            if (!validPassword)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                await userManager.ResetPasswordAsync(adminUser, token, DefaultAdminPassword);
            }

            if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
            {
                await userManager.AddToRoleAsync(adminUser, AdminRole);
            }
        }
    }
}
