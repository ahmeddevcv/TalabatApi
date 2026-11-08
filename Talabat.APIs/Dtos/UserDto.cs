using System.ComponentModel.DataAnnotations;

namespace Talabat.APIs.Dtos
{
    public class UserDto
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        public string? PictureUrl { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class UserProfileUpdateDto
    {
        [Required]
        [MinLength(2, ErrorMessage = "Display name must be at least 2 characters.")]
        public string DisplayName { get; set; }

        public string? PhoneNumber { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters.")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; }
    }

    public class DeleteAccountDto
    {
        [Required]
        public string Password { get; set; }
    }
}
