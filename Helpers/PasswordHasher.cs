using System.Security.Cryptography;

namespace SystemResourceMonitorAPI.Helpers
{
    /// <summary>
    /// Безпечне хешування паролів з використанням PBKDF2
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        /// <summary>
        /// Хешує пароль з автоматично згенерованою сіллю
        /// </summary>
        public static string HashPassword(string password)
        {
            // Генеруємо криптографічно безпечну сіль
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Хешуємо пароль з сіллю використовуючи PBKDF2
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Комбінуємо сіль і хеш для збереження
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Перевіряє чи відповідає пароль хешу
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                // Отримуємо bytes з Base64
                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                // Витягуємо сіль з початку
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Хешуємо введений пароль з тією ж сіллю
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Порівнюємо хеші (constant-time comparison для захисту від timing attacks)
                for (int i = 0; i < HashSize; i++)
                {
                    if (hashBytes[i + SaltSize] != hash[i])
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
