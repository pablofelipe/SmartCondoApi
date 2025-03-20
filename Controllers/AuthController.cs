using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartCondoApi.Models;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(UserManager<User> userManager) : ControllerBase
    {
        private readonly UserManager<User> _userManager = userManager;

        [HttpGet("confirm-email")]
        public async Task<ActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                user.Enabled = true;
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                return Ok("E-mail confirmado com sucesso.");
            }

            return BadRequest("Erro ao confirmar o e-mail.");
        }
    }
}
