using SmartCondoApi.Models;

namespace SmartCondoApi.GraphQL.Mutations
{

    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class VehicleMutations
    {
        public async Task<Vehicle> AddVehicle(
            [Service] SmartCondoContext context,
            VehicleInput input)
        {
            var vehicle = new Vehicle
            {
                Type = input.Type,
                LicensePlate = input.LicensePlate,
                Brand = input.Brand,
                Model = input.Model,
                Color = input.Color,
                UserId = input.UserId,
                Enabled = true
            };

            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<Vehicle> UpdateVehicle(
            [Service] SmartCondoContext context,
            int id,
            VehicleInput input)
        {
            var vehicle = await context.Vehicles.FindAsync(id);
            if (vehicle == null)
                throw new GraphQLException("Vehicle not found!");

            vehicle.Type = input.Type;
            vehicle.LicensePlate = input.LicensePlate;
            vehicle.Brand = input.Brand;
            vehicle.Model = input.Model;
            vehicle.Color = input.Color;
            vehicle.UserId = input.UserId;

            await context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<bool> ToggleVehicleStatus(
            [Service] SmartCondoContext context,
            int id)
        {
            var vehicle = await context.Vehicles.FindAsync(id);
            if (vehicle == null)
                throw new GraphQLException("Vehicle not found!");

            vehicle.Enabled = !vehicle.Enabled;
            await context.SaveChangesAsync();
            return vehicle.Enabled;
        }
    }

    // Input Type (DTO para mutations)
    public record VehicleInput(
        VehicleType Type,
        string LicensePlate,
        string Brand,
        string Model,
        string Color,
        long UserId);
}
