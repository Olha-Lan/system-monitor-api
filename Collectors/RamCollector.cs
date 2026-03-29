using System;

namespace SystemResourceMonitorAPI.Collectors
{
    public class RamCollector
    {
        public double GetTotalMemoryMb()
        {
            try
            {
                var totalMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
                return Math.Round(totalMemory / 1024.0 / 1024.0, 2);
            }
            catch
            {
                return 16384; // Default 16GB
            }
        }

        public double GetUsedMemoryMb()
        {
            try
            {
                var info = GC.GetGCMemoryInfo();
                var used = info.HeapSizeBytes;
                return Math.Round(used / 1024.0 / 1024.0, 2);
            }
            catch
            {
                return 0;
            }
        }

        public double GetAvailableMemoryMb()
        {
            try
            {
                var info = GC.GetGCMemoryInfo();
                var available = info.TotalAvailableMemoryBytes - info.HeapSizeBytes;
                return Math.Round(available / 1024.0 / 1024.0, 2);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Метод що повертає всі метрики одразу
        /// </summary>
        public (double Total, double Used, double Free, double Percent) GetRamMetrics()
        {
            try
            {
                var total = GetTotalMemoryMb();
                var used = GetUsedMemoryMb();
                var free = GetAvailableMemoryMb();
                var percent = total > 0 ? Math.Round((used / total) * 100, 2) : 0;

                return (total, used, free, percent);
            }
            catch
            {
                return (0, 0, 0, 0);
            }
        }
    }
}