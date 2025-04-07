using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCondoApi.Exceptions;
using SmartCondoApi.Services.Vehicle;

namespace SmartCondoApi.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class VehiclesController(IVehicleService _vehicleService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> GetVehicles(
            [FromQuery] string? licensePlate,
            [FromQuery] string? model,
            [FromQuery] int? apartmentNumber,
            [FromQuery] int? parkingSpaceNumber,
            [FromQuery] string? ownerName,
            [FromQuery] string? cpfCnpj)
        {
            try
            {
                var vehicles = await _vehicleService.GetFilteredVehiclesAsync(
                    licensePlate,
                    model,
                    apartmentNumber,
                    parkingSpaceNumber,
                    ownerName,
                    cpfCnpj);

                return Ok(vehicles);
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ex.Message });
            }
        }
    }
}
