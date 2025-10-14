using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Talabat.Core.Entities.Identity;

namespace AdminPanel.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        ///public async Task<IActionResult> Index()
        ///{
        ///    var Users = await userManager.Users.Select(u => new UserViewModel
        ///    {
        ///        Id = u.Id,
        ///        UserName = u.UserName,
        ///        Email = u.Email,
        ///        DisplayName = u.DisplayName,
        ///        Roles = userManager.GetRolesAsync(u).Result
        ///    }).ToListAsync();
         ///
        ///    return View(Users);
        ///}


        public async Task<IActionResult> Index()
        {
            var users = await userManager.Users.ToListAsync();

            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    Roles = roles
                });
            }

            return View(userViewModels);
        }




        //edit user roles
        public async Task<IActionResult> Edit(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            var AllRoles = await roleManager.Roles.ToListAsync();
            var viewModel = new EditUserRolesViewModel()
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = AllRoles.Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    IsSelected = userManager.IsInRoleAsync(user, r.Name).Result
                }).ToList(),
            };

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(EditUserRolesViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);
            var userRole = await userManager.GetRolesAsync(user);
            foreach (var role in model.Roles)
            {
                if (userRole.Any(r => r == role.Name) && !role.IsSelected)
                {
                    await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                if (!userRole.Any(r => r == role.Name) && role.IsSelected)
                {
                    await userManager.AddToRoleAsync(user, role.Name);
                }

            }

            return RedirectToAction(nameof(Index));
        }

    }
}

/*
 * 
 *  public async Task<IActionResult> Edit(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User Not Found");
            var userRoles = await userManager.GetRolesAsync(user);
            var allRoles = roleManager.Roles.Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name,
                IsSelected = userRoles.Contains(r.Name)
            }).ToList();
            var model = new EditUserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = allRoles
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(EditUserRolesViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound("User Not Found");
            var userRoles = await userManager.GetRolesAsync(user);
            var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.Name).ToList();
            var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add roles");
                return View(model);
            }
            result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove roles");
                return View(model);
            }
            return RedirectToAction("Index");
        }
 */
