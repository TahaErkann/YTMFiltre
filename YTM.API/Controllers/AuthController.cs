using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using YTM.Core.Entities;
using YTM.Core.Services;
using BCrypt.Net;

namespace YTM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                _logger.LogInformation($"Register attempt for email: {model.Email}");

                if (await _userService.EmailExistsAsync(model.Email))
                {
                    _logger.LogWarning($"Registration failed: Email already exists - {model.Email}");
                    return BadRequest(new { message = "Bu e-posta adresi zaten kayıtlı!" });
                }

                var user = new User
                {
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "Customer"
                };

                await _userService.CreateUserAsync(user);
                _logger.LogInformation($"User registered successfully: {model.Email}");

                return Ok(new { message = "Kayıt başarılı!" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                return StatusCode(500, new { message = "Kayıt işlemi sırasında bir hata oluştu." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(request.Email);
                if (user == null || !VerifyPassword(request.Password, user.Password))
                {
                    return Unauthorized(new { message = "Geçersiz email veya şifre" });
                }

                if (string.IsNullOrEmpty(user.Id))
                {
                    _logger.LogError($"User ID is null for email: {request.Email}");
                    return StatusCode(500, new { message = "Kullanıcı kimliği bulunamadı" });
                }

                var jwtKey = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                {
                    _logger.LogError("JWT Key is not configured");
                    return StatusCode(500, new { message = "Sunucu yapılandırma hatası" });
                }

                var claims = new[]
                {
                    new Claim("UserId", user.Id),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role ?? "Customer")
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"] ?? "defaultIssuer",
                    audience: _configuration["Jwt:Audience"] ?? "defaultAudience",
                    claims: claims,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: creds
                );

                _logger.LogInformation($"Login successful for {request.Email}. UserId: {user.Id}");

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    role = user.Role,
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return StatusCode(500, new { message = "Giriş işlemi sırasında bir hata oluştu" });
            }
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);
        }
    }

    public class LoginModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
} 