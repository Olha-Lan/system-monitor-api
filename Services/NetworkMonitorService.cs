using SystemResourceMonitorAPI.Collectors;
using SystemResourceMonitorAPI.DTOs;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.Services
{
    /// <summary>
    /// Сервіс моніторингу мережі
    /// </summary>
    public class NetworkMonitorService : INetworkMonitorService
    {
        private readonly NetworkCollector _collector;
        private readonly ILogger<NetworkMonitorService> _logger;

        public NetworkMonitorService(NetworkCollector collector, ILogger<NetworkMonitorService> logger)
        {
            _collector = collector;
            _logger = logger;
        }

        public NetworkMetricsDto GetNetworkMetrics()
        {
            try
            {
                return _collector.GetNetworkMetrics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting network metrics");
                return new NetworkMetricsDto();
            }
        }

        public (double SendRate, double ReceiveRate) GetNetworkSpeed()
        {
            try
            {
                return _collector.GetNetworkSpeed();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting network speed");
                return (0, 0);
            }
        }
    }
}
