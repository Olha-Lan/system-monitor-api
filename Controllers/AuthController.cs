using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemResourceMonitorAPI.Data;
using SystemResourceMonitorAPI.DTOs;
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
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
            {
                _logger.LogWarning("Failed login attempt for user: {Username}", loginDto.Username);
                return Unauthorized(new { message = "Invalid username or password" });
            }

            _logger.LogInformation("User {Username} logged in successfully", loginDto.Username);
            return Ok(result);
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(registerDto);
            if (result == null)
            {
                _logger.LogWarning("Registration failed for user: {Username}", registerDto.Username);
                return BadRequest(new { message = "Username or email already exists" });
            }

            _logger.LogInformation("New user registered: {Username}", registerDto.Username);
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim);
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
    }
}