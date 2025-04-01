using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCondoApi.Dto;
using SmartCondoApi.Models;
using SmartCondoApi.Services.Condominium;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class CondominiumController(ICondominiumService _condominiumService) : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<Condominium>> Get()
        {
            return await _condominiumService.Get();
        }

        // Obter um condominio por ID
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            try
            {
                var condo = await _condominiumService.Get(id);

                return Ok(condo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ex.Message });
            }
        }

        [HttpGet("{condominiumId}/users/search")]
        public async Task<ActionResult> SearchUsers(
            [FromRoute] int condominiumId,
            [FromQuery] UserProfileSearchDTO searchDto)
        {
            try
            {
                var users = await _condominiumService.SearchUsers(condominiumId, searchDto);

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ex.Message });
            }
        }
    }
}
