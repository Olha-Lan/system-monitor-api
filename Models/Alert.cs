using System.ComponentModel.DataAnnotations;

namespace SystemResourceMonitorAPI.Models
{
    /// <summary>
    /// Модель алерту (попередження про перевищення порогів)
    /// </summary>
    public class Alert
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // CPU / RAM / DISK / PROCESS / NETWORK

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime Time { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string Severity { get; set; } = "Info"; // Info / Warning / Critical

        public double CurrentValue { get; set; }

        public double ThresholdValue { get; set; }

        public bool IsResolved { get; set; } = false;

        public DateTime? ResolvedAt { get; set; }
    }
}
