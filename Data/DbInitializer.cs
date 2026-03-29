using SystemResourceMonitorAPI.Helpers;
using SystemResourceMonitorAPI.Models;

namespace SystemResourceMonitorAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            try
            {
                // Перевіряємо чи є вже користувачі
                if (context.Users.Any())
                {
                    return;
                }

                // Створюємо адміністратора
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@systemmonitor.com",
                    PasswordHash = PasswordHasher.HashPassword("Admin123!"),
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var testUser = new User
                {
                    Username = "user",
                    Email = "user@systemmonitor.com",
                    PasswordHash = PasswordHasher.HashPassword("User123!"),
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.AddRange(adminUser, testUser);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
            }
        }
    }
}