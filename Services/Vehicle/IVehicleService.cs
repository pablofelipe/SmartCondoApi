
namespace SmartCondoApi.Services.Vehicle
{
    public interface IVehicleService
    {
        Task<IEnumerable<Models.Vehicle>> GetFilteredVehiclesAsync(string? licensePlate, string? model, int? apartmentNumber, int? parkingSpaceNumber, string? ownerName, string? cpfCnpj);
    }
}
