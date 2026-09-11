using EcommerceAPI.Data;
using EcommerceAPI.DTOs.User;
using EcommerceAPI.Models; // for User
using EcommerceAPI.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text; // for AppDbContext
using Microsoft.Extensions.Options;
using EcommerceAPI.Filters;
using Microsoft.AspNetCore.RateLimiting;
using EcommerceAPI.Constants;
using Microsoft.Extensions.Caching.Memory;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<UserController> _logger;
        private readonly IMemoryCache _cache;

        // 透過建構函式注入 IConfiguration
        //DI
        public UserController(AppDbContext context, IOptions<JwtSettings> jwtOptions, ILogger<UserController> logger, IMemoryCache cache)
        {
            _context = context;
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
            _cache = cache;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return BadRequest("Username already exists.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest("Email already exists.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var user = new User
            {
                Username = registerDto.Username,
                PasswordHash = passwordHash,
                Email = registerDto.Email
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created, new RegisterResponseDto
            {
                Id = user.Id,
                Name = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreateAt = user.CreatedAt
            });
        }

        [HttpPost("Login")]
        [EnableRateLimiting(RateLimitPolicies.Login)]
        [ServiceFilter(typeof(ServerTimeHeaderFilter))]
        public async Task<ActionResult<string>> Login([FromBody] LoginDto loginDto)
        {
            var normalizedUsername = loginDto.Username.ToLowerInvariant();

            var cooldownKey = $"login-cooldown:{normalizedUsername}";
            var cacheKey = $"login-failures:{normalizedUsername}";

            if (_cache.TryGetValue(cooldownKey, out _))
            {
                return StatusCode(
                    StatusCodes.Status429TooManyRequests,
                    "Too many login attempts. Please try again later.");
            }

            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user==null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                _cache.TryGetValue(cacheKey, out int failureCount);

                failureCount++;

                _cache.Set(
                    cacheKey,
                    failureCount,
                    TimeSpan.FromMinutes(10));

                if (failureCount >= 10)
                {
                    _cache.Set(
                    cooldownKey,
                    true,
                    TimeSpan.FromMinutes(1));
                }

                _logger.LogWarning("Login failed due to invalid credentials.");

                return Unauthorized("Invalid username or password.");
            }


            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            // Claims：把已驗證的使用者身份與授權資訊放入 Token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(1), // 通行證有效期為 1 天
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) // 用 HMAC-SHA256 演算法簽名
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);//put header and payload context into token
            string jwtToken = tokenHandler.WriteToken(token); //Signature from token calculation
            // 💡 5. 記錄log information並將令牌發放給前端
            _logger.LogInformation("User {UserId} logged in successfully", user.Id);
            _cache.Remove(cooldownKey);
            _cache.Remove(cacheKey);
            return Ok(new { token = jwtToken, message = "登入成功！" });
        }
    }
}
