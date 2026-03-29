using SystemResourceMonitorAPI.Collectors;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.Services
{
    /// <summary>
    /// Сервіс моніторингу RAM
    /// </summary>
    public class RamMonitorService : IRamMonitorService
    {
        private readonly RamCollector _collector;
        private readonly ILogger<RamMonitorService> _logger;

        public RamMonitorService(RamCollector collector, ILogger<RamMonitorService> logger)
        {
            _collector = collector;
            _logger = logger;
        }

        public double GetTotalMemoryMb()
        {
            try
            {
                return _collector.GetTotalMemoryMb();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total memory");
                return 0;
            }
        }

        public double GetUsedMemoryMb()
        {
            try
            {
                return _collector.GetUsedMemoryMb();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting used memory");
                return 0;
            }
        }

        public double GetFreeMemoryMb()
        {
            try
            {
                return _collector.GetAvailableMemoryMb();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting free memory");
                return 0;
            }
        }

        /// <summary>
        /// Оптимізований метод - отримує всі RAM метрики одним викликом
        /// </summary>
        public (double Total, double Used, double Free, double Percent) GetRamMetrics()
        {
            try
            {
                return _collector.GetRamMetrics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting RAM metrics");
                return (0, 0, 0, 0);
            }
        }
    }
}
