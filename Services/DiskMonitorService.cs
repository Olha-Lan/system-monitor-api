using SystemResourceMonitorAPI.Collectors;
using SystemResourceMonitorAPI.DTOs;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.Services
{
    /// <summary>
    /// Сервіс моніторингу дисків
    /// </summary>
    public class DiskMonitorService : IDiskMonitorService
    {
        private readonly DiskCollector _collector;
        private readonly ILogger<DiskMonitorService> _logger;

        public DiskMonitorService(DiskCollector collector, ILogger<DiskMonitorService> logger)
        {
            _collector = collector;
            _logger = logger;
        }

        public List<DiskMetricsDto> GetAllDisks()
        {
            try
            {
                return _collector.GetAllDisks();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all disks");
                return new List<DiskMetricsDto>();
            }
        }

        public DiskMetricsDto? GetMostFullDisk()
        {
            try
            {
                return _collector.GetMostFullDisk();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most full disk");
                return null;
            }
        }

        public List<DiskMetricsDto> GetCriticalDisks(double threshold = 90.0)
        {
            try
            {
                return _collector.GetCriticalDisks(threshold);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting critical disks");
                return new List<DiskMetricsDto>();
            }
        }
    }
}
