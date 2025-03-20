using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartCondoApi.Exceptions;
using SmartCondoApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartCondoApi.Services
{
    public class UserService(IUserDependencies dependencies)
    {
        private readonly IUserDependencies _dependencies = dependencies;

        public async Task<string> Login([FromBody] Dictionary<string, string> body)
        {
            if (!body.TryGetValue("user", out string userText) || string.IsNullOrEmpty(userText))
            {
                throw new InvalidCredentialsException("Email é obrigatório.");
            }

            if (!body.TryGetValue("secret", out string secretText) || string.IsNullOrEmpty(secretText))
            {
                throw new InvalidCredentialsException("Senha é obrigatória.");
            }

            var user = await _dependencies.UserManager.FindByEmailAsync(userText);

            if (null == user)
            {
                throw new UserNotFoundException("Usuário não encontrado.");
            }

            if (user.EmailConfirmed == false)
            {
                throw new UnconfirmedEmailException("O email não foi validado.");
            }

            if (user.Enabled == false)
            {
                throw new UserDisabledException("Usuário desabilitado.");
            }

            if (await _dependencies.UserManager.IsLockedOutAsync(user))
            {
                throw new UserLockedException("Conta bloqueada. Tente novamente mais tarde.");
            }

            //throw new UserExpiredException("Usuário expirado.");

            if (!await _dependencies.UserManager.CheckPasswordAsync(user, secretText))
            {
                throw new IncorrectPasswordException("Senha incorreta.");
            }

            var userProfile = await _dependencies.Context.UserProfiles.FirstOrDefaultAsync(x => x.Id == user.Id);

            if (null == userProfile)
            {
                throw new UserNotFoundException("Perfil de usuário não encontrado.");
            }

            var dbUserType = await _dependencies.Context.UserTypes.FirstOrDefaultAsync(us => us.Id == userProfile.UserTypeId);

            if (null == dbUserType)
            {
                throw new UserTypeNotFoundException("Tipo de Usuário não encontrado.");
            }

            return GenerateJwtToken(user, dbUserType.Name);
        }


        private string GenerateJwtToken(User user, string userType)
        {
            var configuration = _dependencies.Configuration;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, userType) // Exemplo de role
            };

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1), // Tempo de expiração do token
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
