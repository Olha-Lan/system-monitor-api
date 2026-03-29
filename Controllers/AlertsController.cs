using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemResourceMonitorAPI.Collectors;
using SystemResourceMonitorAPI.DTOs;
using SystemResourceMonitorAPI.Helpers;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.Controllers
{
    /// <summary>
    /// Контролер для управління алертами
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertService _alertsService;
        private readonly ICpuMonitorService _cpuService;
        private readonly IRamMonitorService _ramService;
        private readonly IDiskMonitorService _diskService;
        private readonly ILogger<AlertsController> _logger;

        public AlertsController(
            IAlertService alertsService,
            ICpuMonitorService cpuService,
            IRamMonitorService ramService,
            IDiskMonitorService diskService,
            ILogger<AlertsController> logger)
        {
            _alertsService = alertsService;
            _cpuService = cpuService;
            _ramService = ramService;
            _diskService = diskService;
            _logger = logger;
        }

        /// <summary>
        /// Отримує поточні активні алерти
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AlertDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAlerts()
        {
            try
            {
                var activeAlerts = await _alertsService.GetActiveAlertsAsync();
                
                return Ok(new
                {
                    timestamp = DateTime.UtcNow,
                    alertCount = activeAlerts.Count,
                    alerts = activeAlerts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alerts");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримує зведення по алертам
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(AlertSummaryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAlertSummary()
        {
            try
            {
                var summary = await _alertsService.GetAlertSummaryAsync();
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting alert summary");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримує поточні порогові значення
        /// </summary>
        [HttpGet("thresholds")]
        [ProducesResponseType(typeof(AlertThresholdDto), StatusCodes.Status200OK)]
        public IActionResult GetThresholds()
        {
            try
            {
                return Ok(new AlertThresholdDto
                {
                    CpuWarning = ThresholdHelper.CpuWarningThreshold,
                    CpuCritical = ThresholdHelper.CpuCriticalThreshold,
                    RamWarning = ThresholdHelper.RamWarningThreshold,
                    RamCritical = ThresholdHelper.RamCriticalThreshold,
                    DiskWarning = ThresholdHelper.DiskWarningThreshold,
                    DiskCritical = ThresholdHelper.DiskCriticalThreshold
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting thresholds");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Оновлює порогові значення (тільки для Admin)
        /// </summary>
        [HttpPost("thresholds")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult UpdateThresholds([FromBody] AlertThresholdDto thresholds)
        {
            try
            {
                ThresholdHelper.CpuWarningThreshold = thresholds.CpuWarning;
                ThresholdHelper.CpuCriticalThreshold = thresholds.CpuCritical;
                ThresholdHelper.RamWarningThreshold = thresholds.RamWarning;
                ThresholdHelper.RamCriticalThreshold = thresholds.RamCritical;
                ThresholdHelper.DiskWarningThreshold = thresholds.DiskWarning;
                ThresholdHelper.DiskCriticalThreshold = thresholds.DiskCritical;

                _logger.LogInformation("Thresholds updated successfully");
                return Ok(new { message = "Thresholds updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating thresholds");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Скидає порогові значення до дефолтних
        /// </summary>
        [HttpPost("thresholds/reset")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ResetThresholds()
        {
            try
            {
                ThresholdHelper.ResetToDefaults();
                _logger.LogInformation("Thresholds reset to defaults");
                return Ok(new { message = "Thresholds reset to defaults" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting thresholds");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
