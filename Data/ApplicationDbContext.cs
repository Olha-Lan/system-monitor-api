using Microsoft.EntityFrameworkCore;
using SystemResourceMonitorAPI.Models;

namespace SystemResourceMonitorAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<SystemMetric> SystemMetrics { get; set; }
        public DbSet<ProcessMetric> ProcessMetrics { get; set; }
        public DbSet<Alert> Alerts { get; set; }

        // ДОДАЙ ЦЕЙ МЕТОД (якщо його немає):
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Налаштування зв'язку User → SystemMetrics
            modelBuilder.Entity<SystemMetric>()
                .HasOne(m => m.User)
                .WithMany(u => u.Metrics)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}