using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Models;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserTypeController(SmartCondoContext _context) : Controller
    {
        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<UserType>> Get()
        {
            return await _context.UserTypes.ToListAsync();
        }
    }
}
