using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Exceptions;

namespace SmartCondoApi.Services.Auth
{
    public class AuthService(IAuthDependencies _dependencies) : IAuthService
    {
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

            return GenerateToken(user, dbUserType.Name);
        }

        private string GenerateToken(Models.User user, string userType)
        {
            return new Infra.TokenHandler(_dependencies.Configuration).Generate(user.Id.ToString(), user.Email, userType, DateTime.Now.AddDays(1));
        }
    }
}
