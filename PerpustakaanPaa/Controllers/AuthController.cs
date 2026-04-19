using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PerpustakaanPaa.Context;
using PerpustakaanPaa.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PerpustakaanPaa.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly string _connStr;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
            _connStr = config.GetConnectionString("DefaultConnection")!;
        }

        /// <summary>Login dan dapatkan JWT token</summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            try
            {
                var ctx = new AnggotaContext(_connStr);
                var (anggota, roleName) = ctx.Authenticate(dto.Email, dto.Password);

                if (anggota == null)
                    return Unauthorized(ApiResponse.Error("Email atau password salah", 401));

                var token = GenerateToken(anggota, roleName);
                return Ok(ApiResponse.Success(new { token, role = roleName, nama = anggota.nama }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Error($"Gagal login: {ex.Message}", 500));
            }
        }

        private string GenerateToken(Anggota a, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   a.nama),
                new Claim(JwtRegisteredClaimNames.Email, a.email),
                new Claim("id_anggota",                  a.id_anggota.ToString()),
                new Claim(ClaimTypes.Role,               string.IsNullOrEmpty(role) ? "anggota" : role)
            };
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: cred);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}