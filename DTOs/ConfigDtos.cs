namespace SystemResourceMonitorAPI.DTOs
{
    /// <summary>
    /// Конфігурація системи
    /// </summary>
    public class SystemConfigDto
    {
        public int MetricsCollectionIntervalSeconds { get; set; } = 5;
        public int MetricsRetentionCount { get; set; } = 500;
        public int AlertsRetentionCount { get; set; } = 100;
        public bool EnableBackgroundCollection { get; set; } = true;
        public bool EnableDiskMonitoring { get; set; } = true;
        public bool EnableNetworkMonitoring { get; set; } = true;
    }

    /// <summary>
    /// Health Check відповідь
    /// </summary>
    public class HealthCheckDto
    {
        public string Status { get; set; } = "Healthy"; // Healthy, Degraded, Unhealthy
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string> Checks { get; set; } = new();
        public string Version { get; set; } = "1.0.0";
    }

    /// <summary>
    /// DTO для оцінки здоров'я системи
    /// </summary>
    public class HealthScoreDto
    {
        public int OverallScore { get; set; }
        public string OverallStatus { get; set; } = string.Empty;
        
        public int CpuScore { get; set; }
        public string CpuStatus { get; set; } = string.Empty;
        public double CpuUsage { get; set; }
        
        public int RamScore { get; set; }
        public string RamStatus { get; set; } = string.Empty;
        public double RamUsage { get; set; }
        
        public int DiskScore { get; set; }
        public string DiskStatus { get; set; } = string.Empty;
        public double DiskUsage { get; set; }
        
        public List<string> Recommendations { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }
}
