namespace SystemResourceMonitorAPI.DTOs
{
    /// <summary>
    /// Повна відповідь з усіма метриками
    /// </summary>
    public class MetricResponseDto
    {
        public DateTime Timestamp { get; set; }
        public double CpuUsagePercent { get; set; }
        public RamMetricsDto Ram { get; set; } = new();
        public List<DiskMetricsDto> Disks { get; set; } = new();
        public NetworkMetricsDto Network { get; set; } = new();
        public List<ProcessDto> TopProcesses { get; set; } = new();
    }

    public class RamMetricsDto
    {
        public double TotalMb { get; set; }
        public double UsedMb { get; set; }
        public double FreeMb { get; set; }
        public double UsagePercent { get; set; }
    }

    public class DiskMetricsDto
    {
        public string Name { get; set; } = string.Empty;
        public string VolumeLabel { get; set; } = string.Empty;
        public double TotalGb { get; set; }
        public double FreeGb { get; set; }
        public double UsedGb { get; set; }
        public double UsagePercent { get; set; }
        public string DriveType { get; set; } = string.Empty;
    }

    public class NetworkMetricsDto
    {
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
        public double KbSent { get; set; }
        public double KbReceived { get; set; }
        public int ActiveConnections { get; set; }
    }

    public class ProcessDto
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }
        public double MemoryMb { get; set; }
        public double CpuTimeSec { get; set; }
        public int ThreadCount { get; set; }
        public string Status { get; set; } = "Running";
    }

    /// <summary>
    /// Історія метрик для графіків
    /// </summary>
    public class MetricsHistoryDto
    {
        public List<MetricDataPointDto> CpuHistory { get; set; } = new();
        public List<MetricDataPointDto> RamHistory { get; set; } = new();
        public List<MetricDataPointDto> DiskHistory { get; set; } = new();
    }

    public class MetricDataPointDto
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }

    /// <summary>
    /// Статистика за період
    /// </summary>
    public class StatisticsDto
    {
        public CpuStatistics Cpu { get; set; } = new();
        public RamStatistics Ram { get; set; } = new();
        public DiskStatistics Disk { get; set; } = new();
        public int TotalAlerts { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class CpuStatistics
    {
        public double Current { get; set; }
        public double Average { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string Trend { get; set; } = "Stable"; // Increasing, Decreasing, Stable
    }

    public class RamStatistics
    {
        public double Current { get; set; }
        public double Average { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string Trend { get; set; } = "Stable";
    }

    public class DiskStatistics
    {
        public double AverageUsagePercent { get; set; }
        public double LargestDiskUsagePercent { get; set; }
        public string MostFullDisk { get; set; } = string.Empty;
    }
}
