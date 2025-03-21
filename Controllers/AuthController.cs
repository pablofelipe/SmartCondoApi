using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCondoApi.Exceptions;
using SmartCondoApi.Services.Auth;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController(IAuthService _userService) : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] Dictionary<string, string> body)
        {
            try
            {
                var token = await _userService.Login(body);

                return Ok(new { token });
            }
            catch (InvalidCredentialsException ex)
            {
                return BadRequest(new { ex.Message });
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (UserTypeNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (UnconfirmedEmailException ex)
            {
                return Unauthorized(new { ex.Message });
            }
            catch (UserLockedException ex)
            {
                return Unauthorized(new { ex.Message });
            }
            catch (UserDisabledException ex)
            {
                return Unauthorized(new { ex.Message });
            }
            catch (UserExpiredException ex)
            {
                return Unauthorized(new { ex.Message });
            }
            catch (IncorrectPasswordException ex)
            {
                return Unauthorized(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ex.Message });
            }
        }
    }
}
