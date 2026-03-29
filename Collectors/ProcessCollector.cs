using System.Diagnostics;
using SystemResourceMonitorAPI.DTOs;

namespace SystemResourceMonitorAPI.Collectors
{
    /// <summary>
    /// Збір інформації про процеси з безпечним доступом
    /// </summary>
    public class ProcessCollector
    {
        /// <summary>
        /// Отримує топ процесів за використанням пам'яті
        /// </summary>
        public IEnumerable<ProcessDto> GetTopProcessesByMemory(int top = 10)
        {
            try
            {
                var processes = Process.GetProcesses();

                return processes
                    .Where(p => !string.IsNullOrEmpty(GetProcessNameSafe(p)))
                    .OrderByDescending(p => GetMemoryMbSafe(p))
                    .Take(top)
                    .Select(p => new ProcessDto
                    {
                        Name = GetProcessNameSafe(p),
                        Id = GetProcessIdSafe(p),
                        MemoryMb = Math.Round(GetMemoryMbSafe(p), 2),
                        CpuTimeSec = Math.Round(GetCpuTimeSafe(p), 2),
                        ThreadCount = GetThreadCountSafe(p),
                        Status = GetProcessStatusSafe(p)
                    })
                    .ToList();
            }
            catch
            {
                return new List<ProcessDto>();
            }
        }

        /// <summary>
        /// Отримує топ процесів за використанням CPU
        /// </summary>
        public IEnumerable<ProcessDto> GetTopProcessesByCpu(int top = 10)
        {
            try
            {
                var processes = Process.GetProcesses();

                return processes
                    .Where(p => !string.IsNullOrEmpty(GetProcessNameSafe(p)))
                    .OrderByDescending(p => GetCpuTimeSafe(p))
                    .Take(top)
                    .Select(p => new ProcessDto
                    {
                        Name = GetProcessNameSafe(p),
                        Id = GetProcessIdSafe(p),
                        MemoryMb = Math.Round(GetMemoryMbSafe(p), 2),
                        CpuTimeSec = Math.Round(GetCpuTimeSafe(p), 2),
                        ThreadCount = GetThreadCountSafe(p),
                        Status = GetProcessStatusSafe(p)
                    })
                    .ToList();
            }
            catch
            {
                return new List<ProcessDto>();
            }
        }

        /// <summary>
        /// Пошук процесів за іменем
        /// </summary>
        public IEnumerable<ProcessDto> SearchProcesses(string searchTerm)
        {
            try
            {
                var processes = Process.GetProcesses();

                return processes
                    .Where(p => GetProcessNameSafe(p).Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .Select(p => new ProcessDto
                    {
                        Name = GetProcessNameSafe(p),
                        Id = GetProcessIdSafe(p),
                        MemoryMb = Math.Round(GetMemoryMbSafe(p), 2),
                        CpuTimeSec = Math.Round(GetCpuTimeSafe(p), 2),
                        ThreadCount = GetThreadCountSafe(p),
                        Status = GetProcessStatusSafe(p)
                    })
                    .ToList();
            }
            catch
            {
                return new List<ProcessDto>();
            }
        }

        /// <summary>
        /// Отримує деталі процесу по ID
        /// </summary>
        public ProcessDto? GetProcessById(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                return new ProcessDto
                {
                    Name = GetProcessNameSafe(process),
                    Id = GetProcessIdSafe(process),
                    MemoryMb = Math.Round(GetMemoryMbSafe(process), 2),
                    CpuTimeSec = Math.Round(GetCpuTimeSafe(process), 2),
                    ThreadCount = GetThreadCountSafe(process),
                    Status = GetProcessStatusSafe(process)
                };
            }
            catch
            {
                return null;
            }
        }

        // Безпечні методи доступу до властивостей процесу
        private string GetProcessNameSafe(Process process)
        {
            try { return process.ProcessName; }
            catch { return "Unknown"; }
        }

        private int GetProcessIdSafe(Process process)
        {
            try { return process.Id; }
            catch { return 0; }
        }

        private double GetMemoryMbSafe(Process process)
        {
            try { return process.WorkingSet64 / 1024.0 / 1024.0; }
            catch { return 0; }
        }

        private double GetCpuTimeSafe(Process process)
        {
            try { return process.TotalProcessorTime.TotalSeconds; }
            catch { return 0; }
        }

        private int GetThreadCountSafe(Process process)
        {
            try { return process.Threads.Count; }
            catch { return 0; }
        }

        private string GetProcessStatusSafe(Process process)
        {
            try { return process.Responding ? "Running" : "Not Responding"; }
            catch { return "Unknown"; }
        }
    }
}
