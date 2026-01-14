using BibliotecaApi.Data;
using BibliotecaApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BibliotecaApi.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthController(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>();
        }

        /// <summary>
        /// POST /api/auth/login
        /// Body: { "email": "...", "password": "..." }
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponse { Message = "Dados de login inválidos." });

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user is null)
                return Unauthorized(new AuthResponse { Message = "Credenciais inválidas." });

            var verify = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );

            if (verify == PasswordVerificationResult.Failed)
                return Unauthorized(new AuthResponse { Message = "Credenciais inválidas." });

            var jwtSection = _configuration.GetSection("JwtSettings");
            var key = jwtSection["Key"];
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var expireMinutesStr = jwtSection["ExpireMinutes"];

            if (string.IsNullOrWhiteSpace(key) ||
                string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience) ||
                string.IsNullOrWhiteSpace(expireMinutesStr))
            {
                return StatusCode(500, new AuthResponse
                {
                    Message = "JwtSettings inválido no appsettings.json."
                });
            }

            if (!int.TryParse(expireMinutesStr, out var expireMinutes))
                return StatusCode(500, new AuthResponse
                {
                    Message = "JwtSettings:ExpireMinutes deve ser inteiro."
                });

            if (key.Length < 32)
                return StatusCode(500, new AuthResponse
                {
                    Message = "JwtSettings:Key precisa ter pelo menos 32 caracteres."
                });

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddMinutes(expireMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiration,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new AuthResponse
            {
                Message = "Login efetuado com sucesso.",
                Token = tokenString,
                Expiration = expiration
            });
        }
    }
}
