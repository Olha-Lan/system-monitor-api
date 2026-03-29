using Microsoft.AspNetCore.SignalR;
using SystemResourceMonitorAPI.DTOs;

namespace SystemResourceMonitorAPI.Hubs
{
    /// <summary>
    /// SignalR Hub для real-time оновлення метрик
    /// Автоматично пушить дані на Frontend без polling
    /// </summary>
    // [Authorize]  ⬅️ ЗАКОМЕНТОВАНО! Тепер працює без авторизації
    public class MetricsHub : Hub
    {
        private readonly ILogger<MetricsHub> _logger;

        public MetricsHub(ILogger<MetricsHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Викликається коли клієнт підключається
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Викликається коли клієнт відключається
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Відправляє метрики всім підключеним клієнтам
        /// </summary>
        public async Task BroadcastMetrics(MetricResponseDto metrics)
        {
            await Clients.All.SendAsync("ReceiveMetrics", metrics);
        }

        /// <summary>
        /// Відправляє алерт всім підключеним клієнтам
        /// </summary>
        public async Task BroadcastAlert(AlertDto alert)
        {
            await Clients.All.SendAsync("ReceiveAlert", alert);
        }
    }
}