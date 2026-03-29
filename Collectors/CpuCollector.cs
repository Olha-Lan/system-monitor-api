using System.Diagnostics;

namespace SystemResourceMonitorAPI.Collectors
{
    /// <summary>
    /// Збір метрик CPU з оптимізацією
    /// </summary>
    public class CpuCollector : IDisposable
    {
        private readonly PerformanceCounter _cpuCounter;
        private bool _disposed = false;

        public CpuCollector()
        {
            // Ініціалізація лічильника CPU для всієї системи
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            
            // Перший виклик для ініціалізації (завжди повертає 0)
            _cpuCounter.NextValue();
        }

        /// <summary>
        /// Отримує поточне використання CPU у відсотках
        /// </summary>
        public double GetCpuUsage()
        {
            try
            {
                // Мінімальна затримка для точного вимірювання
                Thread.Sleep(100);
                var value = _cpuCounter.NextValue();
                return Math.Round(value, 2);
            }
            catch (Exception)
            {
                // При помилці повертаємо 0 замість краша
                return 0;
            }
        }

        /// <summary>
        /// Асинхронна версія отримання CPU usage
        /// </summary>
        public async Task<double> GetCpuUsageAsync()
        {
            try
            {
                await Task.Delay(100);
                var value = _cpuCounter.NextValue();
                return Math.Round(value, 2);
            }
            catch (Exception)
            {
                return 0;
            }
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
                {
                    _cpuCounter?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
