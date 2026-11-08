using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities.Identity;

namespace Talabat.Repository.Identity
{
    public static class AppIdentityDbContextSeed
    {
        public static async Task SeedUsersAsync(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppIdentityDbContext? dbContext = null)
        {
            if (dbContext != null)
            {
                try
                {
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'PictureUrl') " +
                        "BEGIN ALTER TABLE AspNetUsers ADD PictureUrl nvarchar(max) NULL; END");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Identity Seed] Column check info: {ex.Message}");
                }
            }

            // Seed Roles
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed Admin User
            if (await userManager.FindByEmailAsync("admin@talabat.com") == null)
            {
                var adminUser = new AppUser()
                {
                    DisplayName = "Admin",
                    Email = "admin@talabat.com",
                    UserName = "admin",
                    PhoneNumber = "01000000000"
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Seed Default User
            if (await userManager.FindByEmailAsync("Ahmed.Shaban@linkdev.com") == null)
            {
                var user = new AppUser()
                {
                    DisplayName = "Ahmed Shaban",
                    Email = "Ahmed.Shaban@linkdev.com",
                    UserName = "Ahmed.shaban",
                    PhoneNumber = "01129841926"
                };
                var result = await userManager.CreateAsync(user, "Pa$$w0rd");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "User");
            }
        }
    }
}
