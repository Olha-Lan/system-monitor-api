using System.ComponentModel.DataAnnotations;

namespace SystemResourceMonitorAPI.Models
{
    /// <summary>
    /// Модель користувача системи
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "User"; // User, Admin

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // ДОДАЙ ЦЕЙ РЯДОК:
        public ICollection<SystemMetric> Metrics { get; set; } = new List<SystemMetric>();
    }
}