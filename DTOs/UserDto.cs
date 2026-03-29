namespace SystemResourceMonitorAPI.DTOs
{
    public class MetricUploadDto
    {
        public double CpuUsagePercent { get; set; }
        public double TotalMemoryMb { get; set; }
        public double UsedMemoryMb { get; set; }
        public double FreeMemoryMb { get; set; }
        public double MemoryUsagePercent { get; set; }
        public double? DiskUsagePercent { get; set; }
        public double? DiskTotalGb { get; set; }
        public double? DiskFreeGb { get; set; }
        public long? NetworkBytesSent { get; set; }
        public long? NetworkBytesReceived { get; set; }
        public int ActiveProcessCount { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}