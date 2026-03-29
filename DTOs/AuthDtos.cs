using System.ComponentModel.DataAnnotations;

namespace SystemResourceMonitorAPI.DTOs
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateUsernameDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string NewUsername { get; set; } = string.Empty;
    }

    public class UpdateEmailDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string NewEmail { get; set; } = string.Empty;
    }

    public class UpdatePasswordDto
    {
        [Required]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
