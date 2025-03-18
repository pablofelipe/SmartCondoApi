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
            if (!body.TryGetValue("user", out string user) || string.IsNullOrEmpty(user))
                throw new InvalidCredentialsException("Email é obrigatório.");

            if (!body.TryGetValue("secret", out string secret) || string.IsNullOrEmpty(secret))
                throw new InvalidCredentialsException("Senha é obrigatória.");

            var dbLogin = await _context.Logins.FirstOrDefaultAsync(x => x.Email == user);

            if (null == dbLogin)
                throw new UserNotFoundException("Login não encontrado.");

            if (dbLogin.IsEmailConfirmed == false)
                throw new UnconfirmedEmailException("O email não foi validado.");

            if (dbLogin.Enabled == false)
                throw new UserDisabledException("Usuário desabilitado.");

            DateOnly dateOnlyToday = DateOnly.FromDateTime(DateTime.Now);

            if (dbLogin.Expiration < dateOnlyToday)
                throw new UserExpiredException("Usuário expirado.");

            var securitHandler = new SecurityHandler();

            string text = securitHandler.EncryptText(secret);

            if (text != dbLogin.Password)
                throw new IncorrectPasswordException("Senha incorreta.");

            var dbUser = await _context.Users.FirstOrDefaultAsync(x => x.UserId == dbLogin.UserId);

            if (null == dbUser)
                throw new UserNotFoundException("Usuário não encontrado.");

            var dbUserType = await _context.UserTypes.FirstOrDefaultAsync(us => us.UserTypeId == dbUser.UserTypeId);

            if (null == dbUserType)
                throw new UserNotFoundException("Tipo de Usuário não encontrado.");

            return GenerateJwtToken(dbLogin, dbUserType.Name);
        }


        private string GenerateJwtToken(Login login, string userType)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, login.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, login.Email),
                new Claim(ClaimTypes.Role, userType) // Exemplo de role
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
