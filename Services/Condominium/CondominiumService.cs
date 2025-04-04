using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Dto;
using SmartCondoApi.Exceptions;
using SmartCondoApi.Models;

namespace SmartCondoApi.Services.Condominium
{
    public class CondominiumService(SmartCondoContext _context) : ICondominiumService
    {
        public async Task<IEnumerable<Models.Condominium>> Get()
        {
            return await _context.Condominiums.ToListAsync();
        }

        public async Task<Models.Condominium> Get(int condominiumId)
        {
            return await _context.Condominiums.FindAsync(condominiumId);
        }

        public async Task<List<UserProfileResponseDTO>> SearchUsers(int condominiumId, UserProfileSearchDTO searchDto)
        {
            if (condominiumId < 1)
            {
                throw new InconsistentDataException($"Numero do condominio {condominiumId} incorreto.");
            }

            if (string.IsNullOrEmpty(searchDto.Name) && string.IsNullOrEmpty(searchDto.RegistrationNumber))
            {
                throw new InconsistentDataException("Nome ou CPF/CNPJ devem ser informados");
            }

            var query = _context.UserProfiles.Where(u => 
                        u.CondominiumId == condominiumId 
                        && null != u.User 
                        && u.User.Enabled == true
                        && u.UserTypeId == searchDto.Type || searchDto.Type == 0);

            if (!string.IsNullOrEmpty(searchDto.Name))
            {
                query = query.Where(u => EF.Functions.Like(u.Name.ToLower(), $"%{searchDto.Name.ToLower()}%"));
            }

            if (!string.IsNullOrEmpty(searchDto.RegistrationNumber))
            {
                query = query.Where(u => u.RegistrationNumber == searchDto.RegistrationNumber);
            }

            var users = await query.ToListAsync();

            var usersDto = users.Select(u => new UserProfileResponseDTO
            {
                Id = u.Id,
                Name = u.Name,
                RegistrationNumber = u.RegistrationNumber,
                UserTypeId = u.UserTypeId,
                CondominiumId = condominiumId,
                FloorId = u.FloorNumber,
                Apartment = u.Apartment,
                ParkingSpaceNumber = u.ParkingSpaceNumber,
            }).ToList();

            return usersDto;
        }
    }
}
