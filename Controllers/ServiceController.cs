using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Models;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ServiceController(SmartCondoContext _context) : Controller
    {

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<Service>> Get()
        {
            return await _context.Services.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            if (id < 1)
                return BadRequest();
            var userLogin = await _context.Services.FirstOrDefaultAsync(m => m.ServiceId == id);
            if (userLogin == null)
                return NotFound();
            return Ok(userLogin);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(Service service)
        {
            _context.Add(service);
            await _context.SaveChangesAsync();
            return Ok();
        }


        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put(Service serviceData)
        {
            if (serviceData == null || serviceData.ServiceId == 0)
                return BadRequest();

            var service = await _context.Services.FindAsync(serviceData.ServiceId);
            if (service == null)
                return NotFound();

            service.ServiceDate = serviceData.ServiceDate;
            service.Description = serviceData.Description;
            service.ServiceData = serviceData.ServiceData;
            service.ServiceType = serviceData.ServiceType;
            service.User = serviceData.User;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            if (id < 1)
                return BadRequest();
            var UserLogin = await _context.Users.FindAsync(id);
            if (UserLogin == null)
                return NotFound();
            _context.Users.Remove(UserLogin);
            await _context.SaveChangesAsync();
            return Ok();

        }
    }
}
