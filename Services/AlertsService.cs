using Microsoft.EntityFrameworkCore;
using SystemResourceMonitorAPI.Data;
using SystemResourceMonitorAPI.DTOs;
using SystemResourceMonitorAPI.Helpers;
using SystemResourceMonitorAPI.Models;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.Services
{
    /// <summary>
    /// Сервіс управління алертами
    /// </summary>
    public class AlertsService : IAlertService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AlertsService> _logger;
        private static readonly List<Alert> _activeAlerts = new();
        private static readonly object _lock = new();

        public AlertsService(ApplicationDbContext context, ILogger<AlertsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<Alert> CheckCpu(double cpuUsage)
        {
            var alerts = new List<Alert>();

            if (ThresholdHelper.IsAboveWarning(cpuUsage, ThresholdHelper.CpuWarningThreshold))
            {
                var severity = ThresholdHelper.GetSeverity(
                    cpuUsage,
                    ThresholdHelper.CpuWarningThreshold,
                    ThresholdHelper.CpuCriticalThreshold
                );

                var alert = new Alert
                {
                    Type = "CPU",
                    Message = $"High CPU usage detected: {cpuUsage:F2}%",
                    Time = DateTime.UtcNow,
                    Severity = severity,
                    CurrentValue = cpuUsage,
                    ThresholdValue = severity == "Critical"
                        ? ThresholdHelper.CpuCriticalThreshold
                        : ThresholdHelper.CpuWarningThreshold
                };

                alerts.Add(alert);
                AddToActiveAlerts(alert);
            }

            return alerts;
        }

        public List<Alert> CheckRam(double usedPercent)
        {
            var alerts = new List<Alert>();

            if (ThresholdHelper.IsAboveWarning(usedPercent, ThresholdHelper.RamWarningThreshold))
            {
                var severity = ThresholdHelper.GetSeverity(
                    usedPercent,
                    ThresholdHelper.RamWarningThreshold,
                    ThresholdHelper.RamCriticalThreshold
                );

                var alert = new Alert
                {
                    Type = "RAM",
                    Message = $"High RAM usage detected: {usedPercent:F2}%",
                    Time = DateTime.UtcNow,
                    Severity = severity,
                    CurrentValue = usedPercent,
                    ThresholdValue = severity == "Critical"
                        ? ThresholdHelper.RamCriticalThreshold
                        : ThresholdHelper.RamWarningThreshold
                };

                alerts.Add(alert);
                AddToActiveAlerts(alert);
            }

            return alerts;
        }

        public List<Alert> CheckDisk(List<DiskMetricsDto> disks)
        {
            var alerts = new List<Alert>();

            foreach (var disk in disks)
            {
                if (ThresholdHelper.IsAboveWarning(disk.UsagePercent, ThresholdHelper.DiskWarningThreshold))
                {
                    var severity = ThresholdHelper.GetSeverity(
                        disk.UsagePercent,
                        ThresholdHelper.DiskWarningThreshold,
                        ThresholdHelper.DiskCriticalThreshold
                    );

                    var alert = new Alert
                    {
                        Type = "DISK",
                        Message = $"High disk usage on {disk.Name} ({disk.VolumeLabel}): {disk.UsagePercent:F2}%",
                        Time = DateTime.UtcNow,
                        Severity = severity,
                        CurrentValue = disk.UsagePercent,
                        ThresholdValue = severity == "Critical"
                            ? ThresholdHelper.DiskCriticalThreshold
                            : ThresholdHelper.DiskWarningThreshold
                    };

                    alerts.Add(alert);
                    AddToActiveAlerts(alert);
                }
            }

            return alerts;
        }

        public async Task<List<AlertDto>> GetActiveAlertsAsync()
        {
            try
            {
                lock (_lock)
                {
                    // Очищуємо старі алерти (старше 5 хвилин)
                    _activeAlerts.RemoveAll(a => (DateTime.UtcNow - a.Time).TotalMinutes > 5);

                    return _activeAlerts.Select(a => new AlertDto
                    {
                        Id = a.Id,
                        Type = a.Type,
                        Message = a.Message,
                        Time = a.Time,
                        Severity = a.Severity,
                        CurrentValue = a.CurrentValue,
                        ThresholdValue = a.ThresholdValue,
                        IsResolved = a.IsResolved
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active alerts");
                return new List<AlertDto>();
            }
        }

        public async Task<AlertSummaryDto> GetAlertSummaryAsync()
        {
            try
            {
                var activeAlerts = await GetActiveAlertsAsync();

                return new AlertSummaryDto
                {
                    TotalAlerts = activeAlerts.Count,
                    CriticalAlerts = activeAlerts.Count(a => a.Severity == "Critical"),
                    WarningAlerts = activeAlerts.Count(a => a.Severity == "Warning"),
                    InfoAlerts = activeAlerts.Count(a => a.Severity == "Info"),
                    RecentAlerts = activeAlerts.OrderByDescending(a => a.Time).Take(10).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert summary");
                return new AlertSummaryDto();
            }
        }

        private void AddToActiveAlerts(Alert alert)
        {
            lock (_lock)
            {
                // Додаємо новий алерт
                _activeAlerts.Add(alert);

                // Зберігаємо максимум 100 алертів
                if (_activeAlerts.Count > 100)
                {
                    _activeAlerts.RemoveAt(0);
                }
            }
        }
    }
}
