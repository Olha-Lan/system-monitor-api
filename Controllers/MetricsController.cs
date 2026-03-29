using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemResourceMonitorAPI.Data;
using SystemResourceMonitorAPI.DTOs;
using SystemResourceMonitorAPI.Models;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.Controllers
{
    /// <summary>
    /// Контролер для отримання системних метрик
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MetricsController : ControllerBase
    {
        private readonly ICpuMonitorService _cpuService;
        private readonly IRamMonitorService _ramService;
        private readonly IDiskMonitorService _diskService;
        private readonly INetworkMonitorService _networkService;
        private readonly IProcessMonitorService _processService;
        private readonly IMetricsHistoryService _historyService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MetricsController> _logger;

        public MetricsController(
            ICpuMonitorService cpuService,
            IRamMonitorService ramService,
            IDiskMonitorService diskService,
            INetworkMonitorService networkService,
            IProcessMonitorService processService,
            IMetricsHistoryService historyService,
            ApplicationDbContext context,
            ILogger<MetricsController> logger)
        {
            _cpuService = cpuService;
            _ramService = ramService;
            _diskService = diskService;
            _networkService = networkService;
            _processService = processService;
            _historyService = historyService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Отримує всі поточні метрики (CPU, RAM, Disk, Network, Processes)
        /// </summary>
        [HttpGet("current")]
        [ProducesResponseType(typeof(MetricResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentMetrics()
        {
            try
            {
                var cpuUsage = await _cpuService.GetCpuUsageAsync();
                var ramMetrics = _ramService.GetRamMetrics();
                var disks = _diskService.GetAllDisks();
                var network = _networkService.GetNetworkMetrics();
                var topProcesses = _processService.GetTopProcessesByMemory(10).ToList();

                var response = new MetricResponseDto
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

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current metrics");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримує тільки CPU метрику
        /// </summary>
        [HttpGet("cpu")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCpuUsage()
        {
            try
            {
                var cpuUsage = await _cpuService.GetCpuUsageAsync();
                return Ok(new
                {
                    timestamp = DateTime.UtcNow,
                    cpuUsagePercent = Math.Round(cpuUsage, 2)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CPU usage");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримує тільки RAM метрику
        /// </summary>
        [HttpGet("ram")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetRamUsage()
        {
            try
            {
                var ramMetrics = _ramService.GetRamMetrics();
                return Ok(new
                {
                    timestamp = DateTime.UtcNow,
                    totalMb = ramMetrics.Total,
                    usedMb = ramMetrics.Used,
                    freeMb = ramMetrics.Free,
                    usagePercent = ramMetrics.Percent
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting RAM usage");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримує метрики всіх дисків
        /// </summary>
        [HttpGet("disks")]
        [ProducesResponseType(typeof(List<DiskMetricsDto>), StatusCodes.Status200OK)]
        public IActionResult GetDisks()
        {
            try
            {
                var disks = _diskService.GetAllDisks();
                return Ok(new
                {
                    timestamp = DateTime.UtcNow,
                    disks = disks
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting disk metrics");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримує мережеві метрики
        /// </summary>
        [HttpGet("network")]
        [ProducesResponseType(typeof(NetworkMetricsDto), StatusCodes.Status200OK)]
        public IActionResult GetNetwork()
        {
            try
            {
                var network = _networkService.GetNetworkMetrics();
                return Ok(new
                {
                    timestamp = DateTime.UtcNow,
                    network = network
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting network metrics");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримує топ процесів
        /// </summary>
        [HttpGet("processes")]
        [ProducesResponseType(typeof(List<ProcessDto>), StatusCodes.Status200OK)]
        public IActionResult GetTopProcesses([FromQuery] int count = 10, [FromQuery] string sortBy = "memory")
        {
            try
            {
                IEnumerable<ProcessDto> processes;

                if (sortBy.Equals("cpu", StringComparison.OrdinalIgnoreCase))
                    processes = _processService.GetTopProcessesByCpu(count);
                else
                    processes = _processService.GetTopProcessesByMemory(count);

                return Ok(new
                {
                    timestamp = DateTime.UtcNow,
                    processes = processes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top processes");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Пошук процесів за іменем
        /// </summary>
        [HttpGet("processes/search")]
        [ProducesResponseType(typeof(List<ProcessDto>), StatusCodes.Status200OK)]
        public IActionResult SearchProcesses([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                    return BadRequest(new { message = "Search term is required" });

                var processes = _processService.SearchProcesses(term);
                return Ok(new
                {
                    timestamp = DateTime.UtcNow,
                    searchTerm = term,
                    processes = processes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching processes");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримує історію метрик за останні N хвилин
        /// </summary>
        [HttpGet("history")]
        [ProducesResponseType(typeof(MetricsHistoryDto), StatusCodes.Status200OK)]
        public IActionResult GetHistory([FromQuery] int minutes = 10)
        {
            try
            {
                if (minutes < 1 || minutes > 60)
                    return BadRequest(new { message = "Minutes must be between 1 and 60" });

                var history = _historyService.GetHistory(minutes);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics history");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримує статистику за період
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(StatisticsDto), StatusCodes.Status200OK)]
        public IActionResult GetStatistics([FromQuery] int minutes = 10)
        {
            try
            {
                if (minutes < 1 || minutes > 60)
                    return BadRequest(new { message = "Minutes must be between 1 and 60" });

                var statistics = _historyService.GetStatistics(minutes);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Завантаження метрик від Python desktop програми
        /// </summary>
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadMetrics([FromBody] MetricUploadDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null) return Unauthorized();

                var userId = int.Parse(userIdClaim);

                var metric = new SystemMetric
                {
                    UserId = userId,
                    Timestamp = DateTime.UtcNow,
                    CpuUsagePercent = dto.CpuUsagePercent,
                    TotalMemoryMb = dto.TotalMemoryMb,
                    UsedMemoryMb = dto.UsedMemoryMb,
                    FreeMemoryMb = dto.FreeMemoryMb,
                    MemoryUsagePercent = dto.MemoryUsagePercent,
                    DiskUsagePercent = dto.DiskUsagePercent,
                    DiskTotalGb = dto.DiskTotalGb,
                    DiskFreeGb = dto.DiskFreeGb,
                    NetworkBytesSent = dto.NetworkBytesSent,
                    NetworkBytesReceived = dto.NetworkBytesReceived,
                    ActiveProcessCount = dto.ActiveProcessCount
                };

                _context.SystemMetrics.Add(metric);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Metrics saved", timestamp = metric.Timestamp });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading metrics");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримати мої метрики з БД (для сайту)
        /// </summary>
        [HttpGet("my")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyMetrics([FromQuery] int hours = 24)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null) return Unauthorized();

                var userId = int.Parse(userIdClaim);
                var since = DateTime.UtcNow.AddHours(-hours);

                var metrics = await _context.SystemMetrics
                .Where(m => m.UserId == userId && m.Timestamp >= since)
                .OrderByDescending(m => m.Timestamp)
                .Take(2000)
                .ToListAsync();

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my metrics");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}