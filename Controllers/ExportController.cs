using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.Controllers
{
    /// <summary>
    /// Контролер для експорту даних (CSV, JSON)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExportController : ControllerBase
    {
        private readonly IMetricsHistoryService _historyService;
        private readonly ILogger<ExportController> _logger;

        public ExportController(IMetricsHistoryService historyService, ILogger<ExportController> logger)
        {
            _historyService = historyService;
            _logger = logger;
        }

        /// <summary>
        /// Експорт історії метрик у форматі JSON
        /// </summary>
        [HttpGet("json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ExportJson([FromQuery] int minutes = 60)
        {
            try
            {
                var history = _historyService.GetHistory(minutes);
                var json = JsonSerializer.Serialize(history, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });

                var bytes = Encoding.UTF8.GetBytes(json);
                var fileName = $"metrics-export-{DateTime.UtcNow:yyyy-MM-dd-HH-mm}.json";

                return File(bytes, "application/json", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting JSON");
                return StatusCode(500, new { message = "Error exporting data" });
            }
        }

        /// <summary>
        /// Експорт історії метрик у форматі CSV
        /// </summary>
        [HttpGet("csv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ExportCsv([FromQuery] int minutes = 60)
        {
            try
            {
                var history = _historyService.GetHistory(minutes);
                var csv = new StringBuilder();

                // Header
                csv.AppendLine("Timestamp,CPU %,RAM %,Disk %");

                // Data
                for (int i = 0; i < history.CpuHistory.Count; i++)
                {
                    var cpuValue = history.CpuHistory.ElementAtOrDefault(i)?.Value ?? 0;
                    var ramValue = history.RamHistory.ElementAtOrDefault(i)?.Value ?? 0;
                    var diskValue = history.DiskHistory.ElementAtOrDefault(i)?.Value ?? 0;
                    var timestamp = history.CpuHistory.ElementAtOrDefault(i)?.Timestamp ?? DateTime.UtcNow;

                    csv.AppendLine($"{timestamp:yyyy-MM-dd HH:mm:ss},{cpuValue},{ramValue},{diskValue}");
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                var fileName = $"metrics-export-{DateTime.UtcNow:yyyy-MM-dd-HH-mm}.csv";

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting CSV");
                return StatusCode(500, new { message = "Error exporting data" });
            }
        }

        /// <summary>
        /// Експорт статистики у форматі JSON
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ExportStatistics([FromQuery] int minutes = 60)
        {
            try
            {
                var statistics = _historyService.GetStatistics(minutes);
                var json = JsonSerializer.Serialize(statistics, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });

                var bytes = Encoding.UTF8.GetBytes(json);
                var fileName = $"statistics-export-{DateTime.UtcNow:yyyy-MM-dd-HH-mm}.json";

                return File(bytes, "application/json", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting statistics");
                return StatusCode(500, new { message = "Error exporting data" });
            }
        }
    }
}
