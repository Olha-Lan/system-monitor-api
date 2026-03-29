using System.Net.NetworkInformation;
using SystemResourceMonitorAPI.DTOs;

namespace SystemResourceMonitorAPI.Collectors
{
    /// <summary>
    /// Збір метрик мережі
    /// </summary>
    public class NetworkCollector
    {
        private long _previousBytesSent = 0;
        private long _previousBytesReceived = 0;
        private DateTime _previousCheck = DateTime.UtcNow;

        /// <summary>
        /// Отримує поточні мережеві метрики
        /// </summary>
        public NetworkMetricsDto GetNetworkMetrics()
        {
            try
            {
                long totalBytesSent = 0;
                long totalBytesReceived = 0;

                // Отримуємо статистику по всіх мережевих інтерфейсах
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (var networkInterface in interfaces)
                {
                    // Пропускаємо неактивні інтерфейси
                    if (networkInterface.OperationalStatus != OperationalStatus.Up)
                        continue;

                    // Пропускаємо loopback
                    if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                        continue;

                    var stats = networkInterface.GetIPv4Statistics();
                    totalBytesSent += stats.BytesSent;
                    totalBytesReceived += stats.BytesReceived;
                }

                // Конвертуємо в KB для зручності
                var kbSent = totalBytesSent / 1024.0;
                var kbReceived = totalBytesReceived / 1024.0;

                // Отримуємо кількість активних TCP з'єднань
                var activeConnections = GetActiveTcpConnections();

                return new NetworkMetricsDto
                {
                    BytesSent = totalBytesSent,
                    BytesReceived = totalBytesReceived,
                    KbSent = Math.Round(kbSent, 2),
                    KbReceived = Math.Round(kbReceived, 2),
                    ActiveConnections = activeConnections
                };
            }
            catch
            {
                return new NetworkMetricsDto();
            }
        }

        /// <summary>
        /// Отримує кількість активних TCP з'єднань
        /// </summary>
        private int GetActiveTcpConnections()
        {
            try
            {
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                var connections = properties.GetActiveTcpConnections();
                return connections.Count(c => c.State == TcpState.Established);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Отримує швидкість передачі даних (bytes/sec)
        /// </summary>
        public (double SendRate, double ReceiveRate) GetNetworkSpeed()
        {
            try
            {
                var currentMetrics = GetNetworkMetrics();
                var now = DateTime.UtcNow;
                var timeDiff = (now - _previousCheck).TotalSeconds;

                if (timeDiff > 0 && _previousBytesSent > 0)
                {
                    var sendRate = (currentMetrics.BytesSent - _previousBytesSent) / timeDiff;
                    var receiveRate = (currentMetrics.BytesReceived - _previousBytesReceived) / timeDiff;

                    _previousBytesSent = currentMetrics.BytesSent;
                    _previousBytesReceived = currentMetrics.BytesReceived;
                    _previousCheck = now;

                    return (Math.Round(sendRate, 2), Math.Round(receiveRate, 2));
                }

                _previousBytesSent = currentMetrics.BytesSent;
                _previousBytesReceived = currentMetrics.BytesReceived;
                _previousCheck = now;

                return (0, 0);
            }
            catch
            {
                return (0, 0);
            }
        }
    }
}
