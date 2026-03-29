using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemResourceMonitorAPI.DTOs;
using SystemResourceMonitorAPI.Helpers;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.Controllers
{
    /// <summary>
    /// Контролер для загального стану системи та рекомендацій
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SystemController : ControllerBase
    {
        private readonly ICpuMonitorService _cpuService;
        private readonly IRamMonitorService _ramService;
        private readonly IDiskMonitorService _diskService;
        private readonly ILogger<SystemController> _logger;

        public SystemController(
            ICpuMonitorService cpuService,
            IRamMonitorService ramService,
            IDiskMonitorService diskService,
            ILogger<SystemController> logger)
        {
            _cpuService = cpuService;
            _ramService = ramService;
            _diskService = diskService;
            _logger = logger;
        }

        /// <summary>
        /// Отримує загальну оцінку здоров'я системи (0-100)
        /// </summary>
        [HttpGet("health-score")]
        [ProducesResponseType(typeof(HealthScoreDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHealthScore()
        {
            try
            {
                var cpuUsage = await _cpuService.GetCpuUsageAsync();
                var ramMetrics = _ramService.GetRamMetrics();
                var disks = _diskService.GetAllDisks();
                var mostFullDisk = disks.OrderByDescending(d => d.UsagePercent).FirstOrDefault();

                // Розрахунок оцінок для кожного компонента (0-100)
                var cpuScore = CalculateCpuScore(cpuUsage);
                var ramScore = CalculateRamScore(ramMetrics.Percent);
                var diskScore = CalculateDiskScore(mostFullDisk?.UsagePercent ?? 0);

                // Загальна оцінка (середнє зважене)
                var overallScore = (int)((cpuScore * 0.4) + (ramScore * 0.3) + (diskScore * 0.3));

                // Статуси
                var cpuStatus = GetStatus(cpuScore);
                var ramStatus = GetStatus(ramScore);
                var diskStatus = GetStatus(diskScore);
                var overallStatus = GetStatus(overallScore);

                // Рекомендації
                var recommendations = GenerateRecommendations(cpuUsage, ramMetrics.Percent, mostFullDisk?.UsagePercent ?? 0);

                var healthScore = new HealthScoreDto
                {
                    OverallScore = overallScore,
                    OverallStatus = overallStatus,
                    CpuScore = cpuScore,
                    CpuStatus = cpuStatus,
                    CpuUsage = Math.Round(cpuUsage, 2),
                    RamScore = ramScore,
                    RamStatus = ramStatus,
                    RamUsage = Math.Round(ramMetrics.Percent, 2),
                    DiskScore = diskScore,
                    DiskStatus = diskStatus,
                    DiskUsage = Math.Round(mostFullDisk?.UsagePercent ?? 0, 2),
                    Recommendations = recommendations,
                    Timestamp = DateTime.UtcNow
                };

                return Ok(healthScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating health score");
                return StatusCode(500, new { message = "Error calculating health score" });
            }
        }

        /// <summary>
        /// Розрахунок оцінки CPU (100 = ідеально, 0 = критично)
        /// </summary>
        private int CalculateCpuScore(double cpuUsage)
        {
            if (cpuUsage < 30) return 100;
            if (cpuUsage < 50) return 90;
            if (cpuUsage < 70) return 75;
            if (cpuUsage < 85) return 50;
            if (cpuUsage < 95) return 25;
            return 10;
        }

        /// <summary>
        /// Розрахунок оцінки RAM
        /// </summary>
        private int CalculateRamScore(double ramUsage)
        {
            if (ramUsage < 50) return 100;
            if (ramUsage < 70) return 85;
            if (ramUsage < 85) return 65;
            if (ramUsage < 95) return 40;
            return 15;
        }

        /// <summary>
        /// Розрахунок оцінки Disk
        /// </summary>
        private int CalculateDiskScore(double diskUsage)
        {
            if (diskUsage < 50) return 100;
            if (diskUsage < 70) return 90;
            if (diskUsage < 85) return 70;
            if (diskUsage < 95) return 45;
            return 20;
        }

        /// <summary>
        /// Визначення статусу на основі оцінки
        /// </summary>
        private string GetStatus(int score)
        {
            if (score >= 85) return "Excellent";
            if (score >= 70) return "Good";
            if (score >= 50) return "Fair";
            if (score >= 30) return "Poor";
            return "Critical";
        }

        /// <summary>
        /// Генерація рекомендацій на основі метрик
        /// </summary>
        private List<string> GenerateRecommendations(double cpuUsage, double ramUsage, double diskUsage)
        {
            var recommendations = new List<string>();

            // CPU рекомендації
            if (cpuUsage > ThresholdHelper.CpuCriticalThreshold)
            {
                recommendations.Add($"⚠️ CPU usage is critical ({cpuUsage:F1}%). Close unnecessary applications.");
            }
            else if (cpuUsage > ThresholdHelper.CpuWarningThreshold)
            {
                recommendations.Add($"⚡ CPU usage is high ({cpuUsage:F1}%). Consider closing some programs.");
            }

            // RAM рекомендації
            if (ramUsage > ThresholdHelper.RamCriticalThreshold)
            {
                recommendations.Add($"⚠️ RAM usage is critical ({ramUsage:F1}%). Close heavy applications or add more RAM.");
            }
            else if (ramUsage > ThresholdHelper.RamWarningThreshold)
            {
                recommendations.Add($"⚡ RAM usage is high ({ramUsage:F1}%). Consider closing unused applications.");
            }

            // Disk рекомендації
            if (diskUsage > ThresholdHelper.DiskCriticalThreshold)
            {
                recommendations.Add($"⚠️ Disk space is critical ({diskUsage:F1}%). Clean up files or add more storage.");
            }
            else if (diskUsage > ThresholdHelper.DiskWarningThreshold)
            {
                recommendations.Add($"⚡ Disk space is running low ({diskUsage:F1}%). Consider cleaning temporary files.");
            }

            // Позитивні рекомендації
            if (recommendations.Count == 0)
            {
                recommendations.Add("✅ System is running optimally. No action needed.");
            }

            return recommendations;
        }
    }
}
