using SystemResourceMonitorAPI.DTOs;
using SystemResourceMonitorAPI.Models;

namespace SystemResourceMonitorAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginDto);
        Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto registerDto);
    }

    public interface ICpuMonitorService
    {
        Task<double> GetCpuUsageAsync();
        double GetCpuUsage();
    }

    public interface IRamMonitorService
    {
        double GetTotalMemoryMb();
        double GetUsedMemoryMb();
        double GetFreeMemoryMb();
        (double Total, double Used, double Free, double Percent) GetRamMetrics();
    }

    public interface IProcessMonitorService
    {
        IEnumerable<ProcessDto> GetTopProcessesByMemory(int count = 10);
        IEnumerable<ProcessDto> GetTopProcessesByCpu(int count = 10);
        IEnumerable<ProcessDto> SearchProcesses(string searchTerm);
        ProcessDto? GetProcessById(int processId);
    }

    public interface IDiskMonitorService
    {
        List<DiskMetricsDto> GetAllDisks();
        DiskMetricsDto? GetMostFullDisk();
        List<DiskMetricsDto> GetCriticalDisks(double threshold = 90.0);
    }

    public interface INetworkMonitorService
    {
        NetworkMetricsDto GetNetworkMetrics();
        (double SendRate, double ReceiveRate) GetNetworkSpeed();
    }

    public interface IAlertService
    {
        List<Alert> CheckCpu(double cpuUsage);
        List<Alert> CheckRam(double usedPercent);
        List<Alert> CheckDisk(List<DiskMetricsDto> disks);
        Task<List<AlertDto>> GetActiveAlertsAsync();
        Task<AlertSummaryDto> GetAlertSummaryAsync();
    }

    public interface IMetricsHistoryService
    {
        void AddMetric(MetricResponseDto metric);
        MetricsHistoryDto GetHistory(int minutes);
        StatisticsDto GetStatistics(int minutes);
        void ClearHistory();
    }
}
