
using SmartCondoApi.GraphQL.Inputs;

namespace SmartCondoApi.Services.Vehicle
{
    public interface IVehicleService
    {
        Task<IEnumerable<Models.Vehicle>> GetFilteredVehiclesAsync(VehicleFilterInput filter);
    }
}
