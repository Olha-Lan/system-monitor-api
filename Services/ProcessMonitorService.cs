using SystemResourceMonitorAPI.Collectors;
using SystemResourceMonitorAPI.DTOs;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.Services
{
    /// <summary>
    /// Сервіс моніторингу процесів
    /// </summary>
    public class ProcessMonitorService : IProcessMonitorService
    {
        private readonly ProcessCollector _collector;
        private readonly ILogger<ProcessMonitorService> _logger;

        public ProcessMonitorService(ProcessCollector collector, ILogger<ProcessMonitorService> logger)
        {
            _collector = collector;
            _logger = logger;
        }

        public IEnumerable<ProcessDto> GetTopProcessesByMemory(int count = 10)
        {
            try
            {
                return _collector.GetTopProcessesByMemory(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top processes by memory");
                return new List<ProcessDto>();
            }
        }

        public IEnumerable<ProcessDto> GetTopProcessesByCpu(int count = 10)
        {
            try
            {
                return _collector.GetTopProcessesByCpu(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top processes by CPU");
                return new List<ProcessDto>();
            }
        }

        public IEnumerable<ProcessDto> SearchProcesses(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<ProcessDto>();

                return _collector.SearchProcesses(searchTerm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching processes with term {SearchTerm}", searchTerm);
                return new List<ProcessDto>();
            }
        }

        public ProcessDto? GetProcessById(int processId)
        {
            try
            {
                return _collector.GetProcessById(processId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting process by ID {ProcessId}", processId);
                return null;
            }
        }
    }
}
