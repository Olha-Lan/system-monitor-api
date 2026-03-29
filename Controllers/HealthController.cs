using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemResourceMonitorAPI.Data;
using SystemResourceMonitorAPI.DTOs;

namespace SystemResourceMonitorAPI.Controllers
{
    /// <summary>
    /// Контролер для перевірки здоров'я системи
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Перевіряє стан системи
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(HealthCheckDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHealth()
        {
            var checks = new Dictionary<string, string>();
            var overallStatus = "Healthy";

            try
            {
                // Перевірка підключення до БД
                var canConnect = await _context.Database.CanConnectAsync();
                checks["Database"] = canConnect ? "OK" : "Failed";
                if (!canConnect) overallStatus = "Unhealthy";
            }
            catch (Exception ex)
            {
                checks["Database"] = "Failed";
                overallStatus = "Unhealthy";
                _logger.LogError(ex, "Database health check failed");
            }

            // Перевірка API
            checks["API"] = "OK";

            var health = new HealthCheckDto
            {
                Status = overallStatus,
                Timestamp = DateTime.UtcNow,
                Checks = checks,
                Version = "1.0.0"
            };

            if (overallStatus == "Unhealthy")
                return StatusCode(503, health);

            return Ok(health);
        }
    }
}
