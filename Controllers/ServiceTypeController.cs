using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Models;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ServiceTypeController(SmartCondoContext _context) : Controller
    {
        [HttpGet]
        public async Task<IEnumerable<ServiceType>> Get()
        {
            return await _context.ServiceTypes.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            if (id < 1)
                return BadRequest();
            var serviceType = await _context.ServiceTypes.FirstOrDefaultAsync(m => m.ServiceTypeId == id);
            if (serviceType == null)
                return NotFound();
            return Ok(serviceType);
        }

        [HttpPost]
        public async Task<IActionResult> Post(ServiceType serviceType)
        {
            _context.Add(serviceType);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Put(ServiceType serviceTypeData)
        {
            if (serviceTypeData == null || serviceTypeData.ServiceTypeId == 0)
                return BadRequest();

            var ServiceType = await _context.ServiceTypes.FindAsync(serviceTypeData.ServiceTypeId);
            if (ServiceType == null)
                return NotFound();

            ServiceType.Name = serviceTypeData.Name;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id < 1)
                return BadRequest();
            var ServiceType = await _context.ServiceTypes.FindAsync(id);
            if (ServiceType == null)
                return NotFound();
            _context.ServiceTypes.Remove(ServiceType);
            await _context.SaveChangesAsync();
            return Ok();

        }

    }
}
