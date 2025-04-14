using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Models;
using SmartCondoApi.GraphQL.Inputs;

namespace SmartCondoApi.Services.Vehicle
{
    public class VehicleService(SmartCondoContext _context) : IVehicleService
    {
        public async Task<IEnumerable<Models.Vehicle>> GetFilteredVehiclesAsync(VehicleFilterInput filter)
        {
            var query = _context.Vehicles
                .Include(v => v.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.LicensePlate))
                query = query.Where(v => EF.Functions.ILike(v.LicensePlate, $"%{filter.LicensePlate}%"));

            if (!string.IsNullOrEmpty(filter.Model))
                query = query.Where(v => EF.Functions.ILike(v.Model, $"%{filter.Model}%"));

            if (filter.ApartmentNumber.HasValue)
            {
                var user = await _context.UserProfiles
                    .FirstOrDefaultAsync(u => u.Apartment == filter.ApartmentNumber);

                query = user == null
                    ? query.Where(v => false)
                    : query.Where(v => v.UserId == user.Id);
            }

            if (filter.ParkingSpaceNumber.HasValue)
            {
                var user = await _context.UserProfiles
                    .FirstOrDefaultAsync(u => u.ParkingSpaceNumber == filter.ParkingSpaceNumber);

                query = user == null
                    ? query.Where(v => false)
                    : query.Where(v => v.UserId == user.Id);
            }

            if (!string.IsNullOrEmpty(filter.OwnerName))
            {
                var userIds = await _context.UserProfiles
                    .Where(u => EF.Functions.ILike(u.Name, $"%{filter.OwnerName}%"))
                    .Select(u => u.Id)
                    .ToListAsync();

                query = userIds.Any()
                    ? query.Where(v => userIds.Contains(v.UserId))
                    : query.Where(v => false);
            }

            if (!string.IsNullOrEmpty(filter.RegistrationNumber))
            {
                var user = await _context.UserProfiles
                    .FirstOrDefaultAsync(u => u.RegistrationNumber == filter.RegistrationNumber);

                query = user == null
                    ? query.Where(v => false)
                    : query.Where(v => v.UserId == user.Id);
            }

            return await query.ToListAsync();
        }
    }
}