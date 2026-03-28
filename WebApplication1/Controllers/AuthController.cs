using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string secretKey = "THIS_IS_A_SUPER_SECRET_KEY_123456789";

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // 🔐 Login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

            if (user == null)
                return Unauthorized("Invalid username or password");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role) // 🔥 Role
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                username = user.Username,
                role = user.Role
            });
        }

        // 🆕 Register
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Username == user.Username))
                return BadRequest("Username already exists");

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new
            {
                message = "User created successfully",
                user.Username,
                user.Role
            });
        }
    }

    // 📦 DTO
    public class LoginModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}