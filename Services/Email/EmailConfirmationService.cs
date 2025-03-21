using Microsoft.AspNetCore.Identity;
using SmartCondoApi.Exceptions;

namespace SmartCondoApi.Services.Email
{
    public class EmailConfirmationService(UserManager<Models.User> _userManager) : IEmailConfirmationService
    {
        public async Task ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new UserNotFoundException("Usuário não encontrado.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                throw new ConfirmEmailException("Erro ao confirmar o e-mail.");
            }

            user.Enabled = true;
            user.EmailConfirmed = true;

            await _userManager.UpdateAsync(user);
        }
    }
}
