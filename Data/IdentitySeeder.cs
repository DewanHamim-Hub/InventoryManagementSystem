using IMS.Models;
using Microsoft.AspNetCore.Identity;

namespace IMS.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Ensure roles exist
            string[] roles = { "Manager", "Staff" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole<int>(role));
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to create role '{role}': {errors}");
                    }
                }
            }

            // Default Manager
            const string managerUsername = "manager";
            const string managerPassword = "Manager@123";

            var managerUser = await userManager.FindByNameAsync(managerUsername);
            if (managerUser == null)
            {
                managerUser = new ApplicationUser
                {
                    UserName = managerUsername,
                    Email = "manager@ims.local",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(managerUser, managerPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create Manager user: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(managerUser, "Manager"))
                await userManager.AddToRoleAsync(managerUser, "Manager");

            // Default Staff
            const string staffUsername = "staff";
            const string staffPassword = "Staff@123";

            var staffUser = await userManager.FindByNameAsync(staffUsername);
            if (staffUser == null)
            {
                staffUser = new ApplicationUser
                {
                    UserName = staffUsername,
                    Email = "staff@ims.local",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(staffUser, staffPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create Staff user: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(staffUser, "Staff"))
                await userManager.AddToRoleAsync(staffUser, "Staff");
        }
    }
}