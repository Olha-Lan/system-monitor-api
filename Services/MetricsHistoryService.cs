using System.Collections.Concurrent;
using SystemResourceMonitorAPI.DTOs;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.Services
{
    /// <summary>
    /// Сервіс зберігання історії метрик в пам'яті
    /// Використовує Circular Buffer для оптимізації пам'яті
    /// </summary>
    public class MetricsHistoryService : IMetricsHistoryService
    {
        private readonly ConcurrentQueue<MetricResponseDto> _metricsHistory = new();
        private readonly ILogger<MetricsHistoryService> _logger;
        private readonly int _maxHistoryCount = 500; // Останні 500 записів (~40 хвилин при збиранні кожні 5 сек)

        public MetricsHistoryService(ILogger<MetricsHistoryService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Додає нову метрику до історії
        /// </summary>
        public void AddMetric(MetricResponseDto metric)
        {
            try
            {
                _metricsHistory.Enqueue(metric);

                // Видаляємо найстаріші записи якщо перевищено ліміт
                while (_metricsHistory.Count > _maxHistoryCount)
                {
                    _metricsHistory.TryDequeue(out _);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding metric to history");
            }
        }

        /// <summary>
        /// Отримує історію метрик за останні N хвилин
        /// </summary>
        public MetricsHistoryDto GetHistory(int minutes)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);
                var recentMetrics = _metricsHistory
                    .Where(m => m.Timestamp >= cutoffTime)
                    .OrderBy(m => m.Timestamp)
                    .ToList();

                return new MetricsHistoryDto
                {
                    CpuHistory = recentMetrics.Select(m => new MetricDataPointDto
                    {
                        Timestamp = m.Timestamp,
                        Value = m.CpuUsagePercent
                    }).ToList(),

                    RamHistory = recentMetrics.Select(m => new MetricDataPointDto
                    {
                        Timestamp = m.Timestamp,
                        Value = m.Ram.UsagePercent
                    }).ToList(),

                    DiskHistory = recentMetrics
                        .Where(m => m.Disks.Any())
                        .Select(m => new MetricDataPointDto
                        {
                            Timestamp = m.Timestamp,
                            Value = m.Disks.Max(d => d.UsagePercent)
                        }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics history");
                return new MetricsHistoryDto();
            }
        }

        /// <summary>
        /// Отримує статистику за період
        /// </summary>
        public StatisticsDto GetStatistics(int minutes)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);
                var recentMetrics = _metricsHistory
                    .Where(m => m.Timestamp >= cutoffTime)
                    .ToList();

                if (!recentMetrics.Any())
                {
                    return new StatisticsDto
                    {
                        PeriodStart = cutoffTime,
                        PeriodEnd = DateTime.UtcNow
                    };
                }

                // CPU статистика
                var cpuValues = recentMetrics.Select(m => m.CpuUsagePercent).ToList();
                var cpuTrend = CalculateTrend(cpuValues);

                // RAM статистика
                var ramValues = recentMetrics.Select(m => m.Ram.UsagePercent).ToList();
                var ramTrend = CalculateTrend(ramValues);

                // Disk статистика
                var mostFullDisk = recentMetrics
                    .Where(m => m.Disks.Any())
                    .SelectMany(m => m.Disks)
                    .GroupBy(d => d.Name)
                    .Select(g => new { Name = g.Key, AvgUsage = g.Average(d => d.UsagePercent) })
                    .OrderByDescending(x => x.AvgUsage)
                    .FirstOrDefault();

                return new StatisticsDto
                {
                    Cpu = new CpuStatistics
                    {
                        Current = cpuValues.LastOrDefault(),
                        Average = cpuValues.Average(),
                        Min = cpuValues.Min(),
                        Max = cpuValues.Max(),
                        Trend = cpuTrend
                    },
                    Ram = new RamStatistics
                    {
                        Current = ramValues.LastOrDefault(),
                        Average = ramValues.Average(),
                        Min = ramValues.Min(),
                        Max = ramValues.Max(),
                        Trend = ramTrend
                    },
                    Disk = new DiskStatistics
                    {
                        AverageUsagePercent = mostFullDisk?.AvgUsage ?? 0,
                        LargestDiskUsagePercent = mostFullDisk?.AvgUsage ?? 0,
                        MostFullDisk = mostFullDisk?.Name ?? "N/A"
                    },
                    TotalAlerts = 0, // TODO: додати підрахунок алертів
                    PeriodStart = cutoffTime,
                    PeriodEnd = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating statistics");
                return new StatisticsDto();
            }
        }

        /// <summary>
        /// Очищує всю історію
        /// </summary>
        public void ClearHistory()
        {
            try
            {
                _metricsHistory.Clear();
                _logger.LogInformation("Metrics history cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing metrics history");
            }
        }

        /// <summary>
        /// Розраховує тренд (зростання/зменшення/стабільно)
        /// </summary>
        private string CalculateTrend(List<double> values)
        {
            if (values.Count < 3)
                return "Stable";

            var firstHalf = values.Take(values.Count / 2).Average();
            var secondHalf = values.Skip(values.Count / 2).Average();

            var difference = secondHalf - firstHalf;

            if (difference > 5)
                return "Increasing";
            if (difference < -5)
                return "Decreasing";
            return "Stable";
        }
    }
}
