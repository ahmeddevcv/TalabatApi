using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AdminPanel.Controllers
{
    [Authorize]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager) 
        {
            this.roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            var roles = await roleManager.Roles.ToListAsync();
            return View(roles);
        }
        [HttpPost]
        public async Task<IActionResult> Create(RoleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var roles = await roleManager.Roles.ToListAsync();
                return View(nameof(Index), roles);
            }
            var roleExists = await roleManager.RoleExistsAsync(model.Name);
            if (!roleExists) 
            {
                await roleManager.CreateAsync(new IdentityRole(model.Name.Trim()));
                return RedirectToAction(nameof(Index),await roleManager.Roles.ToListAsync());
            }
            else
            {
                ModelState.AddModelError("", "Role already exists.");
                var roles = await roleManager.Roles.ToListAsync();
                return View(nameof(Index), roles);
            }
        }
        public async Task<IActionResult> Delete(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role != null)
            {
                await roleManager.DeleteAsync(role);
            }
            return RedirectToAction(nameof(Index), await roleManager.Roles.ToListAsync());
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            var model = new RoleViewModel
            {
                Id = role.Id,
                Name = role.Name
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(RoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var role = await roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                return NotFound();
            }
            role.Name = model.Name.Trim();
            var result = await roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index), await roleManager.Roles.ToListAsync());
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }




    }
}
