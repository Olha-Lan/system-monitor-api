using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SystemResourceMonitorAPI.Collectors
{
    public class CpuCollector : IDisposable
    {
        private PerformanceCounter? _cpuCounter;
        private bool _disposed = false;

        public CpuCollector()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    _cpuCounter.NextValue();
                }
                catch
                {
                    _cpuCounter = null;
                }
            }
        }

        public double GetCpuUsage()
        {
            try
            {
                if (_cpuCounter != null)
                {
                    Thread.Sleep(100);
                    return Math.Round(_cpuCounter.NextValue(), 2);
                }
                return GetCpuUsageLinux();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<double> GetCpuUsageAsync()
        {
            try
            {
                if (_cpuCounter != null)
                {
                    await Task.Delay(100);
                    return Math.Round(_cpuCounter.NextValue(), 2);
                }
                return GetCpuUsageLinux();
            }
            catch
            {
                return 0;
            }
        }

        private double GetCpuUsageLinux()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "sh",
                    Arguments = "-c \"top -bn1 | grep 'Cpu(s)' | awk '{print $2}'\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                using var process = Process.Start(startInfo);
                if (process == null) return 0;
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                if (double.TryParse(output.Trim(), out var cpu))
                    return Math.Round(cpu, 2);
            }
            catch { }
            return 0;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    _cpuCounter?.Dispose();
                _disposed = true;
            }
        }
    }
}
