namespace SystemResourceMonitorAPI.Helpers
{
    /// <summary>
    /// Управління пороговими значеннями для алертів
    /// </summary>
    public static class ThresholdHelper
    {
        // CPU порогові значення
        public static double CpuWarningThreshold { get; set; } = 70.0;
        public static double CpuCriticalThreshold { get; set; } = 90.0;

        // RAM порогові значення
        public static double RamWarningThreshold { get; set; } = 80.0;
        public static double RamCriticalThreshold { get; set; } = 95.0;

        // Disk порогові значення
        public static double DiskWarningThreshold { get; set; } = 85.0;
        public static double DiskCriticalThreshold { get; set; } = 95.0;

        /// <summary>
        /// Визначає рівень серйозності на основі значення і порогів
        /// </summary>
        public static string GetSeverity(double value, double warningThreshold, double criticalThreshold)
        {
            if (value >= criticalThreshold)
                return "Critical";
            if (value >= warningThreshold)
                return "Warning";
            return "Info";
        }

        /// <summary>
        /// Перевіряє чи перевищено поріг попередження
        /// </summary>
        public static bool IsAboveWarning(double value, double warningThreshold)
        {
            return value >= warningThreshold;
        }

        /// <summary>
        /// Перевіряє чи перевищено критичний поріг
        /// </summary>
        public static bool IsAboveCritical(double value, double criticalThreshold)
        {
            return value >= criticalThreshold;
        }

        /// <summary>
        /// Скидає порогові значення до дефолтних
        /// </summary>
        public static void ResetToDefaults()
        {
            CpuWarningThreshold = 70.0;
            CpuCriticalThreshold = 90.0;
            RamWarningThreshold = 80.0;
            RamCriticalThreshold = 95.0;
            DiskWarningThreshold = 85.0;
            DiskCriticalThreshold = 95.0;
        }
    }
}
