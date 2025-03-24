using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Models;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TowerController(SmartCondoContext _context) : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<Tower>> Get()
        {
            return await _context.Towers.ToListAsync();
        }

        //// Obter uma torre por ID
        //[HttpGet("{id}")]
        //[Authorize]
        //public async Task<IActionResult> GetTower(int id)
        //{
        //    var tower = await _context.Towers.FindAsync(id);
        //    if (tower == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(tower);
        //}

        [HttpGet("byCondominium/{condominiumId}")]
        [Authorize]
        public async Task<IActionResult> GetTowersByCondominium(int condominiumId)
        {
            var towers = await _context.Towers.Where(t => t.CondominiumId == condominiumId).ToListAsync();

            if (towers == null || towers.Count == 0)
            {
                return NotFound("Nenhuma torre encontrada para o condomínio especificado.");
            }

            return Ok(towers);
        }
    }
}
