using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartCondoApi.Exceptions;
using SmartCondoApi.Infra;
using SmartCondoApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartCondoApi.Services
{
    public class LoginService(SmartCondoContext context, IConfiguration configuration)
    {
        private readonly SmartCondoContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        public async Task<string> Login([FromBody] Dictionary<string, string> body)
        {
            if (!body.TryGetValue("user", out string? user) || !body.TryGetValue("secret", out string? secret))
                throw new InvalidCredentialsException("Email e senha são obrigatórios.");

            var dbData = await _context.Logins.FirstOrDefaultAsync(x => x.Email == user);

            if (null == dbData)
                throw new UserNotFoundException("Login não encontrado.");

            if (dbData.Enabled == false)
                throw new UserDisabledException("Usuário desabilitado.");

            DateOnly dateOnlyToday = DateOnly.FromDateTime(DateTime.Now);

            if (dbData.Expiration < dateOnlyToday)
                throw new UserExpiredException("Usuário expirado.");

            string text = new SecurityHandler().EncryptText(secret);

            if (text != dbData.Password)
                throw new IncorrectPasswordException("Senha incorreta.");

            var dbUser = await _context.Users.FirstOrDefaultAsync(x => x.LoginId == dbData.LoginId);

            if (null == dbUser)
                throw new UserNotFoundException("Usuário não encontrado.");

            return GenerateJwtToken(dbUser);
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
