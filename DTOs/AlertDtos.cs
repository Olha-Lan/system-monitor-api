namespace SystemResourceMonitorAPI.DTOs
{
    public class AlertDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string Severity { get; set; } = string.Empty;
        public double CurrentValue { get; set; }
        public double ThresholdValue { get; set; }
        public bool IsResolved { get; set; }
    }

    public class AlertThresholdDto
    {
        public double CpuWarning { get; set; } = 70;
        public double CpuCritical { get; set; } = 90;
        public double RamWarning { get; set; } = 80;
        public double RamCritical { get; set; } = 95;
        public double DiskWarning { get; set; } = 85;
        public double DiskCritical { get; set; } = 95;
    }

    public class AlertSummaryDto
    {
        public int TotalAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int WarningAlerts { get; set; }
        public int InfoAlerts { get; set; }
        public List<AlertDto> RecentAlerts { get; set; } = new();
    }
}
