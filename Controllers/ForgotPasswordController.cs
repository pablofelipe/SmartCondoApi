using Microsoft.AspNetCore.Mvc;
using SmartCondoApi.Dto;
using SmartCondoApi.Services;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForgotPasswordController(IForgotPasswordService _forgotPasswordService) : ControllerBase
    {
        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            try
            {
                var response = await _forgotPasswordService.SendResetLinkAsync(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                var response = await _forgotPasswordService.ResetPasswordAsync(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}