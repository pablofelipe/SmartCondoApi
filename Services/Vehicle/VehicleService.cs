using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Exceptions;
using SmartCondoApi.Models;

namespace SmartCondoApi.Services.Vehicle
{
    public class VehicleService(SmartCondoContext _context) : IVehicleService
    {
        public async Task<IEnumerable<Models.Vehicle>> GetFilteredVehiclesAsync(
            string? licensePlate = null,
            string? model = null,
            int? apartmentNumber = 0,
            int? parkingSpaceNumber = 0,
            string? ownerName = null,
            string? cpfCnpj = null)
        {
            var query = _context.Vehicles
                .Include(v => v.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(licensePlate))
                query = query.Where(v => v.LicensePlate.Contains(licensePlate));

            if (!string.IsNullOrEmpty(model))
                query = query.Where(v => v.Model.Contains(model));

            if (apartmentNumber != null)
            {
                var user = _context.UserProfiles.FirstOrDefaultAsync(us => us.Apartment == apartmentNumber);

                if (null == user)
                    throw new UserNotFoundException($"Usuário para apartamento {apartmentNumber} não encontrado");

                query = query.Where(us => us.UserId == user.Id);
            }

            if (parkingSpaceNumber != null)
            {
                var user = _context.UserProfiles.FirstOrDefaultAsync(us => us.ParkingSpaceNumber == parkingSpaceNumber);

                if (null == user)
                    throw new UserNotFoundException($"Usuário para vaga {parkingSpaceNumber} não encontrado");

                query = query.Where(us => us.UserId == user.Id);
            }

            if (!string.IsNullOrEmpty(ownerName))
            {
                var userIds = _context.UserProfiles
                    .Where(u => EF.Functions.Like(u.Name.ToLower(), $"%{ownerName.ToLower()}%"))
                    .Select(u => u.Id)
                    .ToList();

                if (!userIds.Any())
                    throw new UserNotFoundException($"Usuário com nome {ownerName} não encontrado");

                query = query.Where(us => userIds.Contains(us.UserId));
            }

            if (!string.IsNullOrEmpty(cpfCnpj))
            {
                var user = _context.UserProfiles.FirstOrDefaultAsync(u => u.RegistrationNumber == cpfCnpj);

                if (null == user)
                    throw new UserNotFoundException($"Usuário com CPF/CNPJ {cpfCnpj} não encontrado");

                query = query.Where(us => us.UserId == user.Id);
            }

            return await query.ToListAsync();
        }
    }
}
