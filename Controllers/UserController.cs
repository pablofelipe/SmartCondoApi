
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Domain;
using SmartCondoApi.dto;
using SmartCondoApi.Models;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController(SmartCondoContext _context) : ControllerBase
    {

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<User>> Get()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            // Garante que o Login.User não seja validado
            user.Login.User = null;
            user.Login.Password = new SecurityHandler().EncryptText(user.Login.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            UserDto? userDto = CreateDto(user);

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, userDto);
        }

        private UserDto? CreateDto(User user)
        {
            return _context.Users
                 .Where(u => u.UserId == user.UserId)
                 .Select(u => new UserDto
                 {
                     UserId = u.UserId,
                     Name = u.Name,
                     Address = u.Address,
                     Type = u.Type,
                     LoginId = u.LoginId,
                     Login = new LoginDto
                     {
                         LoginId = u.Login.LoginId,
                         Email = u.Login.Email,
                         Expiration = u.Login.Expiration,
                         Enabled = u.Login.Enabled
                     }
                 })
                 .FirstOrDefault();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Login) // Carrega o Login relacionado
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            UserDto? userDto = CreateDto(user);

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, userDto);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put([FromBody] User userData)
        {
            if (userData == null || userData.UserId == 0)
                return NotFound("UserId");

            var user = await _context.Users.FindAsync(userData.UserId);
            if (user == null)
                return NotFound();

            user.Name = userData.Name;
            user.Address = userData.Address;
            user.Type = userData.Type;
            user.Login = userData.Login;
            user.Services = userData.Services;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            if (id < 1)
                return BadRequest();
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok();

        }
    }
}
