using System.ComponentModel.DataAnnotations;

namespace SystemResourceMonitorAPI.Models
{
    /// <summary>
    /// Модель метрик процесу
    /// </summary>
    public class ProcessMetric
    {
        [Key]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(200)]
        public string ProcessName { get; set; } = string.Empty;

        public int ProcessId { get; set; }

        // Пам'ять процесу
        public double MemoryUsageMb { get; set; }

        // CPU час процесу (в секундах)
        public double CpuTimeSeconds { get; set; }

        // Кількість потоків
        public int ThreadCount { get; set; }

        public string? Description { get; set; }
    }
}
