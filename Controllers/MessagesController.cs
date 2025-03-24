using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Dto;
using SmartCondoApi.Models;
using SmartCondoApi.Services.Message;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class MessagesController(IMessageService messageService, UserManager<User> userManager, SmartCondoContext context) : ControllerBase
    {
        private readonly IMessageService _messageService = messageService;
        private readonly UserManager<User> _userManager = userManager;
        private readonly SmartCondoContext _context = context;

        [HttpPost]
        public async Task<ActionResult> SendMessage([FromBody] MessageCreateDto messageDto)
        {
            var userId = _userManager.GetUserId(User);
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == Convert.ToInt64(userId));

            if (userProfile == null)
            {
                return Unauthorized();
            }

            try
            {
                var message = await _messageService.SendMessageAsync(messageDto, userProfile.Id);
                return Ok(message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return new ObjectResult(new ProblemDetails
                {
                    Title = "Forbidden",
                    Detail = ex.Message,
                    Status = StatusCodes.Status403Forbidden
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ex.Message } );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ocorreu um erro interno. Mensagem: {ex.Message}" });
            }
        }

        [HttpGet("received")]
        public async Task<ActionResult> GetReceivedMessages()
        {
            var userId = _userManager.GetUserId(User);
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == Convert.ToInt64(userId));

            if (userProfile == null) return Unauthorized();

            var messages = await _messageService.GetReceivedMessagesAsync(userProfile.Id);
            return Ok(messages);
        }

        [HttpGet("sent")]
        public async Task<ActionResult> GetSentMessages()
        {
            var userId = _userManager.GetUserId(User);
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == Convert.ToInt64(userId));

            if (userProfile == null) return Unauthorized();

            var messages = await _messageService.GetSentMessagesAsync(userProfile.Id);
            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetMessage(long id)
        {
            var userId = _userManager.GetUserId(User);
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == Convert.ToInt64(userId));

            if (userProfile == null) return Unauthorized();

            var message = await _messageService.GetMessageAsync(id, userProfile.Id);

            if (message == null) return NotFound();

            return Ok(message);
        }

        [HttpPost("{id}/read")]
        public async Task<ActionResult> MarkAsRead(long id)
        {
            var userId = _userManager.GetUserId(User);
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == Convert.ToInt64(userId));

            if (userProfile == null) return Unauthorized();

            await _messageService.MarkAsReadAsync(id, userProfile.Id);
            return Ok();
        }
    }
}
