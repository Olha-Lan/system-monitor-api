using Microsoft.AspNetCore.SignalR;
using SystemResourceMonitorAPI.DTOs;
using SystemResourceMonitorAPI.Hubs;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.BackgroundJobs
{
    /// <summary>
    /// Background Service для автоматичного збору метрик кожні N секунд
    /// Оптимізований для мінімального впливу на продуктивність системи
    /// + SignalR для real-time push на Frontend
    /// </summary>
    public class MetricsBackgroundService : BackgroundService
    {
        private readonly ILogger<MetricsBackgroundService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHubContext<MetricsHub> _hubContext;
        private int _collectionIntervalSeconds = 5;

        public MetricsBackgroundService(
            ILogger<MetricsBackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory,
            IHubContext<MetricsHub> hubContext)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Metrics Background Service started with SignalR support");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CollectMetricsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Metrics Background Service");
                }

                // Чекаємо наступного циклу
                await Task.Delay(TimeSpan.FromSeconds(_collectionIntervalSeconds), stoppingToken);
            }

            _logger.LogInformation("Metrics Background Service stopped");
        }

        /// <summary>
        /// Збирає всі метрики, зберігає в історію та відправляє через SignalR
        /// </summary>
        private async Task CollectMetricsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            try
            {
                var cpuService = scope.ServiceProvider.GetRequiredService<ICpuMonitorService>();
                var ramService = scope.ServiceProvider.GetRequiredService<IRamMonitorService>();
                var diskService = scope.ServiceProvider.GetRequiredService<IDiskMonitorService>();
                var networkService = scope.ServiceProvider.GetRequiredService<INetworkMonitorService>();
                var processService = scope.ServiceProvider.GetRequiredService<IProcessMonitorService>();
                var historyService = scope.ServiceProvider.GetRequiredService<IMetricsHistoryService>();
                var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();

                // Збираємо метрики (асинхронно де можливо)
                var cpuUsage = await cpuService.GetCpuUsageAsync();
                var ramMetrics = ramService.GetRamMetrics();
                var disks = diskService.GetAllDisks();
                var network = networkService.GetNetworkMetrics();
                var topProcesses = processService.GetTopProcessesByMemory(10).ToList();

                // Створюємо об'єкт метрики
                var metric = new MetricResponseDto
                {
                    Timestamp = DateTime.UtcNow,
                    CpuUsagePercent = Math.Round(cpuUsage, 2),
                    Ram = new RamMetricsDto
                    {
                        TotalMb = ramMetrics.Total,
                        UsedMb = ramMetrics.Used,
                        FreeMb = ramMetrics.Free,
                        UsagePercent = ramMetrics.Percent
                    },
                    Disks = disks,
                    Network = network,
                    TopProcesses = topProcesses
                };

                // Зберігаємо в історію
                historyService.AddMetric(metric);

                // 🔥 НОВИНКА: Відправляємо через SignalR на Frontend!
                await _hubContext.Clients.All.SendAsync("ReceiveMetrics", metric, cancellationToken);

                // Перевіряємо алерти
                var cpuAlerts = alertService.CheckCpu(cpuUsage);
                var ramAlerts = alertService.CheckRam(ramMetrics.Percent);
                var diskAlerts = alertService.CheckDisk(disks);

                // Відправляємо нові алерти через SignalR
                foreach (var alert in cpuAlerts.Concat(ramAlerts).Concat(diskAlerts))
                {
                    var alertDto = new AlertDto
                    {
                        Type = alert.Type,
                        Message = alert.Message,
                        Time = alert.Time,
                        Severity = alert.Severity,
                        CurrentValue = alert.CurrentValue,
                        ThresholdValue = alert.ThresholdValue
                    };
                    await _hubContext.Clients.All.SendAsync("ReceiveAlert", alertDto, cancellationToken);
                }

                _logger.LogDebug("Metrics collected and broadcasted: CPU {Cpu}%, RAM {Ram}%", cpuUsage, ramMetrics.Percent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting metrics");
            }
        }

        /// <summary>
        /// Дозволяє динамічно змінити інтервал збору
        /// </summary>
        public void SetCollectionInterval(int seconds)
        {
            if (seconds >= 1 && seconds <= 60)
            {
                _collectionIntervalSeconds = seconds;
                _logger.LogInformation("Collection interval changed to {Seconds} seconds", seconds);
            }
        }
    }
}
