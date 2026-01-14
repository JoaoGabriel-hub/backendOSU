using BibliotecaApi.Data;
using BibliotecaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        // Injeção do DbContext
        public AuthController(AppDbContext context)
        {
            _context = context;

            // Inicializa o hasher padrão do ASP.NET
            _passwordHasher = new PasswordHasher<User>();
        }

        // -----------------------------
        // POST: api/auth/login
        // -----------------------------
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1️⃣ Validação do payload
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2️⃣ Buscar usuário pelo email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            // 3️⃣ Se usuário não existir
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // 4️⃣ Verificar senha usando hash
            var passwordVerificationResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password
                );

            // 5️⃣ Senha inválida
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // ✅ Login válido
            return Ok(new AuthResponse
            {
                Message = "Login successful. JWT will be generated next."
            });
        }
    }
}
