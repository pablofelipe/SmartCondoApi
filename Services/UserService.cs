using Microsoft.EntityFrameworkCore;
using SmartCondoApi.dto;
using SmartCondoApi.Dto;
using SmartCondoApi.Exceptions;
using SmartCondoApi.Infra;
using SmartCondoApi.Models;

namespace SmartCondoApi.Services
{
    public class UserService(SmartCondoContext context)
    {
        private readonly SmartCondoContext _context = context;

        public async Task<UserResponseDTO> AddUserAsync(UserCreateDTO userCreateDTO)
        {
            // Valida o CPF
            if (!ValidateCPF(userCreateDTO.PersonalTaxId))
            {
                throw new InvalidPersonalTaxIDException("CPF inválido.");
            }

            if (null == userCreateDTO.Login)
            {
                throw new ArgumentException("Nenhum login encontrado");
            }

            // Encripta o password
            userCreateDTO.Login.Password = new SecurityHandler().EncryptText(userCreateDTO.Login.Password);

            var user = new User
            {
                Name = userCreateDTO.Name,
                Address = userCreateDTO.Address,
                UserTypeId = userCreateDTO.UserTypeId,
                PersonalTaxID = userCreateDTO.PersonalTaxId,
                CondominiumId = userCreateDTO.CondominiumId,
                TowerId = userCreateDTO.TowerId,
                FloorId = userCreateDTO.FloorId,
                Apartment = userCreateDTO.Apartment,
                ParkingSpaceNumber = userCreateDTO.ParkingSpaceNumber,
                Login = new Login
                {
                    Email = userCreateDTO.Login.Email,
                    Password = userCreateDTO.Login.Password,
                    Expiration = userCreateDTO.Login.Expiration,
                    Enabled = userCreateDTO.Login.Enabled
                }
            };

            // Adiciona o usuário ao banco de dados
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return new UserResponseDTO()
            {
                Name = userCreateDTO.Name,
                Address = userCreateDTO.Address,
                UserTypeId = userCreateDTO.UserTypeId,
                PersonalTaxId = userCreateDTO.PersonalTaxId,
                CondominiumId = userCreateDTO.CondominiumId,
                TowerId = userCreateDTO.TowerId,
                FloorId = userCreateDTO.FloorId,
                Apartment = userCreateDTO.Apartment,
                ParkingSpaceNumber = userCreateDTO.ParkingSpaceNumber
            };
        }

        private static bool ValidateCPF(string personalTaxID)
        {
            return new IndividualTaxpayerRegistryHandler().Verify(personalTaxID);
        }

        public async Task<UserResponseDTO> UpdateUserAsync(long userId, UserUpdateDTO userUpdateDTO)
        {
            if (null == userUpdateDTO)
                throw new InvalidCredentialsException("Dados de usuario são obrigatórios.");

            // Busca o usuário existente no banco de dados
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (null == user)
                throw new UserNotFoundException("Usuário não encontrado.");

            if (userUpdateDTO.Name != null) user.Name = userUpdateDTO.Name;
            if (userUpdateDTO.Address != null) user.Address = userUpdateDTO.Address;

            if (userUpdateDTO.CondominiumId.HasValue) user.CondominiumId = userUpdateDTO.CondominiumId.Value;
            if (userUpdateDTO.TowerId.HasValue) user.TowerId = userUpdateDTO.TowerId.Value;
            if (userUpdateDTO.FloorId.HasValue) user.FloorId = userUpdateDTO.FloorId.Value;
            if (userUpdateDTO.Apartment.HasValue) user.Apartment = userUpdateDTO.Apartment.Value;

            // Atualiza os dados do Login, se fornecido
            if (userUpdateDTO.Login != null && user.Login != null)
            {
                if (userUpdateDTO.Login.Email != null) user.Login.Email = userUpdateDTO.Login.Email;
                if (userUpdateDTO.Login.Password != null) user.Login.Password = userUpdateDTO.Login.Password;
                if (userUpdateDTO.Login.Expiration.HasValue) user.Login.Expiration = userUpdateDTO.Login.Expiration.Value;
                if (userUpdateDTO.Login.Enabled.HasValue) user.Login.Enabled = userUpdateDTO.Login.Enabled.Value;
            }

            await _context.SaveChangesAsync();

            return new UserResponseDTO()
            {
                Name = user.Name,
                Address = user.Address,
                PersonalTaxId = user.PersonalTaxID,
                CondominiumId = user.CondominiumId,
                TowerId = user.TowerId,
                FloorId = user.FloorId,
                Apartment = user.Apartment
            };
        }
    }
}
