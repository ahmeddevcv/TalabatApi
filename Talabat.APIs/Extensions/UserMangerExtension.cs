using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Talabat.Core.Entities.Identity;

namespace Talabat.APIs.Extensions
{
    public static class UserMangerExtension//8 GetUserAddres
    {
        public static async Task<AppUser?> FindUserWithAddressByEmailAsync(this UserManager<AppUser> userManager,ClaimsPrincipal User)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await userManager.Users.Include(x=>x.Address).FirstOrDefaultAsync(x=>x.Email==email);
            return user;
        }
    }
}
