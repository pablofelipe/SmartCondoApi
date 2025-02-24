
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartCondoApi.Domain;
using SmartCondoApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LoginController(SmartCondoContext dbContext, IConfiguration configuration) : ControllerBase
    {
        private readonly SmartCondoContext _dbContext = dbContext;
        private readonly IConfiguration _configuration = configuration;

        [HttpPost(Name = "login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(string user, string secret)
        {
            var dbData = await _dbContext.Logins.FirstOrDefaultAsync(x => x.Email == user);

            if (null == dbData)
                return NotFound(user);

            if (dbData.Enabled == false)
                return BadRequest("Usuario desabilitado");

            DateOnly dateOnlyToday = DateOnly.FromDateTime(DateTime.Now);

            if (dbData.Expiration < dateOnlyToday)
                return BadRequest("Cadastro de usuario expirado");

            string text = new SecurityHandler().EncryptText(secret);

            if (text != dbData.Password)
                return Unauthorized();

            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.LoginId == dbData.LoginId);

            if (null == dbUser)
                return NotFound(dbUser);

            var token = GenerateJwtToken(dbUser);

            return Ok(new { token });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Login.Email),
                new Claim(ClaimTypes.Role, user.Type == 1 ? "Admin" : "User") // Exemplo de role
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1), // Tempo de expiração do token
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
