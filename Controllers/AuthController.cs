using Microsoft.AspNetCore.Mvc;
using SystemResourceMonitorAPI.DTOs;
using SystemResourceMonitorAPI.Services.Interfaces;

namespace SystemResourceMonitorAPI.Controllers
{
    /// <summary>
    /// Контролер аутентифікації
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Логін користувача
        /// </summary>
        /// <param name="loginDto">Дані для логіну</param>
        /// <returns>JWT токен та інформація про користувача</returns>
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

        /// <summary>
        /// Реєстрація нового користувача
        /// </summary>
        /// <param name="registerDto">Дані для реєстрації</param>
        /// <returns>JWT токен та інформація про нового користувача</returns>
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
    }
}
