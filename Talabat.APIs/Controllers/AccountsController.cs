using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.APIs.Extensions;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Services;

namespace Talabat.APIs.Controllers
{
    public class AccountsController : ApiBaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        private const string UserImagesFolder = "images/users";
        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public AccountsController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IMapper mapper,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _env = env;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null) return BadRequest(new ApiResponse(401));

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded) return BadRequest(new ApiResponse(401));

            return new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PictureUrl = user.PictureUrl,
                Token = await _tokenService.CreateTokenAsync(user, _userManager),
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User"
            };
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto model)
        {
            if (CheckEmailExists(model.Email).Result.Value)
                return BadRequest(new ApiValidationErrorResponse() { Errors = new string[] { "this email is already in user!!" } });

            var user = new AppUser()
            {
                DisplayName = model.DisplayName,
                Email = model.Email,
                UserName = model.Email.Split("@")[0],
                PhoneNumber = model.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return BadRequest(new ApiResponse(401));

            return new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PictureUrl = user.PictureUrl,
                Token = await _tokenService.CreateTokenAsync(user, _userManager),
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User"
            };
        }

        [Authorize]
        [HttpGet("get")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(new ApiResponse(404, "User not found"));

            return new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PictureUrl = user.PictureUrl,
                Token = await _tokenService.CreateTokenAsync(user, _userManager),
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User"
            };
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<UserDto>> UpdateProfile(UserProfileUpdateDto model)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(new ApiResponse(404, "User not found"));

            user.DisplayName = model.DisplayName.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400, "Problem updating user profile"));

            return new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PictureUrl = user.PictureUrl,
                Token = await _tokenService.CreateTokenAsync(user, _userManager),
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User"
            };
        }

        [Authorize]
        [HttpPost("upload-picture")]
        public async Task<ActionResult<object>> UploadProfilePicture(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse(400, "No file uploaded."));

            if (file.Length > MaxImageSizeBytes)
                return BadRequest(new ApiResponse(400, "File size exceeds the 5 MB limit."));

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
                return BadRequest(new ApiResponse(400, "Invalid image format. Allowed: JPG, PNG, WEBP, GIF."));

            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(new ApiResponse(404, "User not found"));

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folder = Path.Combine(webRoot, "images", "users");
            Directory.CreateDirectory(folder);

            // Clean previous picture if it exists
            if (!string.IsNullOrEmpty(user.PictureUrl))
            {
                try
                {
                    var oldRel = user.PictureUrl.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
                    var oldFull = Path.Combine(webRoot, oldRel);
                    if (System.IO.File.Exists(oldFull)) System.IO.File.Delete(oldFull);
                }
                catch { /* Ignore file cleanup failure */ }
            }

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.PictureUrl = $"{UserImagesFolder}/{fileName}";
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400, "Failed to update profile picture."));

            return Ok(new { pictureUrl = user.PictureUrl });
        }

        [Authorize]
        [HttpDelete("picture")]
        public async Task<ActionResult> RemoveProfilePicture()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(new ApiResponse(404, "User not found"));

            if (!string.IsNullOrEmpty(user.PictureUrl))
            {
                try
                {
                    var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var rel = user.PictureUrl.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
                    var full = Path.Combine(webRoot, rel);
                    if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
                }
                catch { /* Ignore cleanup failure */ }
            }

            user.PictureUrl = null;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400, "Failed to remove profile picture."));

            return Ok(new { message = "Profile picture removed successfully" });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto model)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(new ApiResponse(404, "User not found"));

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            return Ok(new { message = "Password changed successfully" });
        }

        [Authorize]
        [HttpPost("delete-account")]
        public async Task<ActionResult> DeleteAccount(DeleteAccountDto model)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound(new ApiResponse(404, "User not found"));

            var checkPassword = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!checkPassword.Succeeded)
                return BadRequest(new ApiResponse(400, "Incorrect password. Cannot delete account."));

            // Clean picture
            if (!string.IsNullOrEmpty(user.PictureUrl))
            {
                try
                {
                    var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var rel = user.PictureUrl.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
                    var full = Path.Combine(webRoot, rel);
                    if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
                }
                catch { }
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400, "Failed to delete account."));

            return Ok(new { message = "Account deleted successfully" });
        }

        [Authorize]
        [HttpGet("address")]
        public async Task<ActionResult<AddressDto>> GetUserAddres()
        {
            var user = await _userManager.FindUserWithAddressByEmailAsync(User);
            if (user == null) return NotFound(new ApiResponse(404, "User not found"));
            var address = _mapper.Map<Address, AddressDto>(user.Address);
            return Ok(address);
        }

        [Authorize]
        [HttpPut("address")]
        public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto updatedAddress)
        {
            var user = await _userManager.FindUserWithAddressByEmailAsync(User);
            if (user == null) return NotFound(new ApiResponse(404, "User not found"));
            var address = _mapper.Map<AddressDto, Address>(updatedAddress);
            if (user.Address == null)
            {
                user.Address = address;
            }
            else
            {
                address.Id = user.Address.Id;
                user.Address = address;
            }
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400));
            return Ok(updatedAddress);
        }

        [HttpGet("emailexists")]
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            return await _userManager.FindByEmailAsync(email) is not null;
        }
    }
}
