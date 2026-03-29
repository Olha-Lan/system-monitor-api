using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemResourceMonitorAPI.Data;
using SystemResourceMonitorAPI.DTOs;
using SystemResourceMonitorAPI.Helpers;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly ApplicationDbContext _context;

        public AuthController(IAuthService authService,
                             ILogger<AuthController> logger,
                             ApplicationDbContext context)
        {
            _authService = authService;
            _logger = logger;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(registerDto);
            if (result == null)
                return BadRequest(new { message = "Username or email already exists" });

            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                role = user.Role,
                createdAt = user.CreatedAt
            });
        }

        [HttpPut("profile/username")]
        [Authorize]
        public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Перевірка чи username вже зайнятий
            var exists = _context.Users.Any(u => u.Username == dto.NewUsername && u.Id != userId);
            if (exists)
                return BadRequest(new { message = "Username already taken" });

            user.Username = dto.NewUsername;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Username updated successfully" });
        }

        [HttpPut("profile/email")]
        [Authorize]
        public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var exists = _context.Users.Any(u => u.Email == dto.NewEmail && u.Id != userId);
            if (exists)
                return BadRequest(new { message = "Email already taken" });

            user.Email = dto.NewEmail;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Email updated successfully" });
        }

        [HttpPut("profile/password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Перевірка старого пароля
            if (!PasswordHasher.VerifyPassword(dto.OldPassword, user.PasswordHash))
                return BadRequest(new { message = "Current password is incorrect" });

            user.PasswordHash = PasswordHasher.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password updated successfully" });
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim == null) return null;
            return int.Parse(claim);
        }
    }
}