using Microsoft.AspNetCore.Identity;
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
        public static async Task SeedUsersAsync(UserManager<AppUser> userManager)
        {
            if (!userManager.Users.Any())
            {
                var user = new AppUser()
                {
                    DisplayName = "Ahmed Shaban",
                    Email = "Ahmed.Shaban@linkdev.com",
                    UserName = "Ahmed.shaban",
                    PhoneNumber = "01129841926"
                };
                await userManager.CreateAsync(user, "Pa$$w0rd");
            }
        }
    }
}
