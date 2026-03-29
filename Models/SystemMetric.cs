using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemResourceMonitorAPI.Models
{
    /// <summary>
    /// Модель системних метрик (CPU, RAM, Disk, Network)
    /// </summary>
    public class SystemMetric
    {
        [Key]
        public int Id { get; set; }

        // ========== ДОДАЙ ЦІ РЯДКИ ==========
        [Required]
        public int UserId { get; set; }  // Чиї це метрики?

        [ForeignKey("UserId")]
        public User? User { get; set; }  // Зв'язок з користувачем
                                         // ====================================

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // CPU метрики
        public double CpuUsagePercent { get; set; }

        // RAM метрики
        public double TotalMemoryMb { get; set; }
        public double UsedMemoryMb { get; set; }
        public double FreeMemoryMb { get; set; }
        public double MemoryUsagePercent { get; set; }

        // Disk метрики (найбільш заповнений диск)
        public double? DiskUsagePercent { get; set; }
        public double? DiskTotalGb { get; set; }
        public double? DiskFreeGb { get; set; }

        // Network метрики
        public long? NetworkBytesSent { get; set; }
        public long? NetworkBytesReceived { get; set; }

        // Додаткова інформація
        public int ActiveProcessCount { get; set; }
        public string? Notes { get; set; }
    }
}