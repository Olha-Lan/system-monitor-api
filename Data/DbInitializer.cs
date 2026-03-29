using SystemResourceMonitorAPI.Helpers;
using SystemResourceMonitorAPI.Models;

namespace SystemResourceMonitorAPI.Data
{
    /// <summary>
    /// Ініціалізація бази даних початковими даними
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Створює початкових користувачів якщо БД порожня
        /// </summary>
        public static void Initialize(ApplicationDbContext context)
        {
            // Переконуємось що база даних створена
            context.Database.EnsureCreated();

            // Перевіряємо чи є вже користувачі
            if (context.Users.Any())
            {
                return; // БД вже ініціалізована
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

            // Створюємо звичайного користувача для тестування
            var testUser = new User
            {
                Username = "user",
                Email = "user@systemmonitor.com",
                PasswordHash = PasswordHasher.HashPassword("User123!"),
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Додаємо користувачів
            context.Users.AddRange(adminUser, testUser);
            context.SaveChanges();

            Console.WriteLine("Database initialized with default users:");
            Console.WriteLine("  Admin: admin / Admin123!");
            Console.WriteLine("  User:  user / User123!");
        }
    }
}
