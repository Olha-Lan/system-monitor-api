using SystemResourceMonitorAPI.Collectors;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.Services
{
    /// <summary>
    /// Сервіс моніторингу CPU
    /// </summary>
    public class CpuMonitorService : ICpuMonitorService
    {
        private readonly CpuCollector _cpuCollector;
        private readonly ILogger<CpuMonitorService> _logger;

        public CpuMonitorService(CpuCollector cpuCollector, ILogger<CpuMonitorService> logger)
        {
            _cpuCollector = cpuCollector;
            _logger = logger;
        }

        /// <summary>
        /// Синхронне отримання CPU usage
        /// </summary>
        public double GetCpuUsage()
        {
            try
            {
                return _cpuCollector.GetCpuUsage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CPU usage");
                return 0;
            }
        }

        /// <summary>
        /// Асинхронне отримання CPU usage (кращий варіант для API)
        /// </summary>
        public async Task<double> GetCpuUsageAsync()
        {
            try
            {
                return await _cpuCollector.GetCpuUsageAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CPU usage asynchronously");
                return 0;
            }
        }
    }
}
