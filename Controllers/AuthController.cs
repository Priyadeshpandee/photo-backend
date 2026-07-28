using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PhotographyCMS.Data;
using PhotographyCMS.DTOs;
using PhotographyCMS.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PhotographyCMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            AppDbContext context,
            IConfiguration configuration
        )
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (existingUser != null)
            {
                return BadRequest("User already exists");
            }

            var hashedPassword =
                BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Email = dto.Email,
                Password = hashedPassword
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User Created Successfully"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid Email");
            }

            bool isPasswordValid =
                BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    user.Password
                );

            if (!isPasswordValid)
            {
                return Unauthorized("Invalid Password");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!
                )
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler()
                    .WriteToken(token)
            });
        }
    }
}