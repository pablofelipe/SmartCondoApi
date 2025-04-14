using SmartCondoApi.GraphQL.Inputs;
using SmartCondoApi.Models;
using SmartCondoApi.Services.Vehicle;

namespace SmartCondoApi.GraphQL.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class VehicleQueries
    {
        //[UsePaging]
        //[UseProjection]
        //[UseFiltering]
        //[UseSorting]
        public async Task<IEnumerable<Vehicle>> GetVehicles(
            [Service] IVehicleService vehicleService,
            [GraphQLType(typeof(VehicleFilterInputType))] VehicleFilterInput? filter = null)
        {
            return await vehicleService.GetFilteredVehiclesAsync(filter ?? new VehicleFilterInput());
        }
    }
}
