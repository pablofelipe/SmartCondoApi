
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Models;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController(SmartCondoContext _context) : ControllerBase
    {

        [HttpGet]
        public async Task<IEnumerable<User>> Get()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            if (id < 1)
                return BadRequest();
            var userLogin = await _context.Users.FirstOrDefaultAsync(m => m.UserId == id);
            if (userLogin == null)
                return NotFound();
            return Ok(userLogin);
        }

        [HttpPost]
        public async Task<IActionResult> Post(User user)
        {
            _context.Add(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Put(User userData)
        {
            if (userData == null || userData.UserId == 0)
                return BadRequest();

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
