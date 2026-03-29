using SystemResourceMonitorAPI.DTOs;

namespace SystemResourceMonitorAPI.Collectors
{
    /// <summary>
    /// Збір метрик дисків
    /// </summary>
    public class DiskCollector
    {
        /// <summary>
        /// Отримує інформацію про всі диски
        /// </summary>
        public List<DiskMetricsDto> GetAllDisks()
        {
            var disks = new List<DiskMetricsDto>();

            try
            {
                var drives = DriveInfo.GetDrives();

                foreach (var drive in drives)
                {
                    try
                    {
                        // Пропускаємо недоступні диски
                        if (!drive.IsReady)
                            continue;

                        var totalGb = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
                        var freeGb = drive.TotalFreeSpace / 1024.0 / 1024.0 / 1024.0;
                        var usedGb = totalGb - freeGb;
                        var usagePercent = totalGb > 0 ? (usedGb / totalGb) * 100 : 0;

                        disks.Add(new DiskMetricsDto
                        {
                            Name = drive.Name,
                            VolumeLabel = string.IsNullOrEmpty(drive.VolumeLabel) ? "Local Disk" : drive.VolumeLabel,
                            TotalGb = Math.Round(totalGb, 2),
                            FreeGb = Math.Round(freeGb, 2),
                            UsedGb = Math.Round(usedGb, 2),
                            UsagePercent = Math.Round(usagePercent, 2),
                            DriveType = drive.DriveType.ToString()
                        });
                    }
                    catch
                    {
                        // Пропускаємо проблемні диски
                        continue;
                    }
                }
            }
            catch
            {
                // Повертаємо порожній список при загальній помилці
            }

            return disks;
        }

        /// <summary>
        /// Отримує інформацію про найбільш заповнений диск
        /// </summary>
        public DiskMetricsDto? GetMostFullDisk()
        {
            var disks = GetAllDisks();
            return disks.OrderByDescending(d => d.UsagePercent).FirstOrDefault();
        }

        /// <summary>
        /// Перевіряє чи є критично заповнені диски
        /// </summary>
        public List<DiskMetricsDto> GetCriticalDisks(double threshold = 90.0)
        {
            return GetAllDisks().Where(d => d.UsagePercent >= threshold).ToList();
        }
    }
}
