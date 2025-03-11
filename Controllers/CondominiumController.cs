using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Models;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CondominiumController(SmartCondoContext _context) : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<Condominium>> Get()
        {
            return await _context.Condominiums.ToListAsync();
        }
    }
}
