using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SystemResourceMonitorAPI.Data;
using SystemResourceMonitorAPI.DTOs;
using SystemResourceMonitorAPI.Helpers;
using SystemResourceMonitorAPI.Models;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.Services
{
    /// <summary>
    /// Сервіс аутентифікації з JWT токенами
    /// Оптимізований для продуктивності та безпеки
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context, 
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Логін користувача з перевіркою паролю
        /// </summary>
        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginDto)
        {
            try
            {
                // Пошук користувача (AsNoTracking для швидкості читання)
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User {Username} not found", loginDto.Username);
                    return null;
                }

                // Перевірка паролю
                if (!PasswordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Invalid password for user {Username}", loginDto.Username);
                    return null;
                }

                // Оновлюємо час останнього логіну (асинхронно)
                var userToUpdate = await _context.Users.FindAsync(user.Id);
                if (userToUpdate != null)
                {
                    userToUpdate.LastLoginAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                // Генеруємо JWT токен
                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.UtcNow.AddHours(24);

                _logger.LogInformation("User {Username} logged in successfully", user.Username);

                return new LoginResponseDto
                {
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", loginDto.Username);
                return null;
            }
        }

        /// <summary>
        /// Реєстрація нового користувача
        /// </summary>
        public async Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto registerDto)
        {
            try
            {
                // Перевірка чи існує користувач з таким username
                if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
                {
                    _logger.LogWarning("Registration failed: Username {Username} already exists", registerDto.Username);
                    return null;
                }

                // Перевірка чи існує користувач з таким email
                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                {
                    _logger.LogWarning("Registration failed: Email {Email} already exists", registerDto.Email);
                    return null;
                }

                // Створюємо нового користувача
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = PasswordHasher.HashPassword(registerDto.Password),
                    Role = "User", // За замовчуванням роль User
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New user registered: {Username}", user.Username);

                // Генеруємо токен для нового користувача
                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.UtcNow.AddHours(24);

                return new LoginResponseDto
                {
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Username}", registerDto.Username);
                return null;
            }
        }

        /// <summary>
        /// Генерація JWT токена
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? "SystemResourceMonitorAPI";
            var audience = jwtSettings["Audience"] ?? "SystemResourceMonitorClient";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
