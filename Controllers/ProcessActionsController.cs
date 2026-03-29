using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace SystemResourceMonitorAPI.Controllers
{
    /// <summary>
    /// Контролер для керування процесами
    /// Дозволяє вбивати/перезапускати процеси
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProcessActionsController : ControllerBase
    {
        private readonly ILogger<ProcessActionsController> _logger;

        public ProcessActionsController(ILogger<ProcessActionsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Вбити процес за ID
        /// </summary>
        [HttpPost("kill/{processId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult KillProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                
                if (process == null)
                {
                    return NotFound(new { message = $"Process with ID {processId} not found" });
                }

                var processName = process.ProcessName;
                
                // Не дозволяємо вбивати критичні системні процеси
                if (IsCriticalProcess(processName))
                {
                    _logger.LogWarning("Attempt to kill critical process: {ProcessName}", processName);
                    return Forbid($"Cannot kill critical system process: {processName}");
                }

                process.Kill();
                process.WaitForExit(5000); // Чекаємо максимум 5 секунд

                _logger.LogInformation("Process killed: {ProcessName} (ID: {ProcessId})", processName, processId);
                
                return Ok(new 
                { 
                    message = $"Process '{processName}' killed successfully",
                    processId = processId,
                    processName = processName
                });
            }
            catch (ArgumentException)
            {
                return NotFound(new { message = $"Process with ID {processId} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error killing process {ProcessId}", processId);
                return StatusCode(500, new { message = "Error killing process", error = ex.Message });
            }
        }

        /// <summary>
        /// Отримати детальну інформацію про процес
        /// </summary>
        [HttpGet("{processId}/details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetProcessDetails(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                
                if (process == null)
                {
                    return NotFound(new { message = $"Process with ID {processId} not found" });
                }

                var details = new
                {
                    id = process.Id,
                    name = process.ProcessName,
                    memoryMb = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2),
                    cpuTimeSec = process.TotalProcessorTime.TotalSeconds,
                    threadCount = process.Threads.Count,
                    handleCount = process.HandleCount,
                    startTime = process.StartTime,
                    priorityClass = process.PriorityClass.ToString(),
                    responding = process.Responding,
                    hasExited = process.HasExited,
                    mainWindowTitle = process.MainWindowTitle,
                    fileName = GetProcessFileName(process)
                };

                return Ok(details);
            }
            catch (ArgumentException)
            {
                return NotFound(new { message = $"Process with ID {processId} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting process details {ProcessId}", processId);
                return StatusCode(500, new { message = "Error getting process details" });
            }
        }

        /// <summary>
        /// Перевіряє чи процес критичний для системи
        /// </summary>
        private bool IsCriticalProcess(string processName)
        {
            var criticalProcesses = new[]
            {
                "system", "csrss", "smss", "services", "lsass", "winlogon",
                "explorer", "svchost", "dwm", "wininit", "RuntimeBroker"
            };

            return criticalProcesses.Contains(processName.ToLower());
        }

        /// <summary>
        /// Безпечне отримання шляху до файлу процесу
        /// </summary>
        private string GetProcessFileName(Process process)
        {
            try
            {
                return process.MainModule?.FileName ?? "N/A";
            }
            catch
            {
                return "Access Denied";
            }
        }
    }
}
