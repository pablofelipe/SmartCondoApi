using Microsoft.EntityFrameworkCore;
using Serilog;
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

            var condo = await _context.Condominiums.FirstOrDefaultAsync(c => c.CondominiumId == userCreateDTO.CondominiumId);

            if (null == condo)
            {
                throw new ArgumentException($"Condominio {userCreateDTO.CondominiumId} não encontrado");
            }

            if (!condo.Enabled)
            {
                throw new CondominiumDisabledException($"Condomínio {condo.Name} desabilitado. Entre em contato com o administrador para mais informações.");
            }

            var count = (from usr in context.Users
                         join login in context.Logins on usr.UserId equals login.UserId
                         where login.Enabled == true && usr.CondominiumId == userCreateDTO.CondominiumId
                         select new
                         {
                             User = usr,
                             Login = login
                         }).Count();

            if (count > condo.MaxUsers)
            {
                throw new UsersExceedException("O número máximo de usuários permitidos para este condomínio foi atingido. Entre em contato com o administrador para mais informações.");
            }

            await ResidentValidations(userCreateDTO, condo);

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

        private async Task ResidentValidations(UserCreateDTO userCreateDTO, Condominium condo)
        {
            var userTypes = await _context.UserTypes.FirstOrDefaultAsync(ut => ut.UserTypeId == userCreateDTO.UserTypeId);

            if (null == userTypes)
            {
                throw new ArgumentException($"Tipo de usuário {userCreateDTO.UserTypeId} não encontrado");
            }

            if (userTypes.Name != "Resident")
                return;

            if (userCreateDTO.Apartment == 0)
            {
                throw new InconsistentDataException($"Número de apartamento incorreto.");
            }

            if (userCreateDTO.TowerId > condo.TowerCount)
            {
                throw new InconsistentDataException($"Número de torre incorreto. O condomínio {condo.Name} possui {condo.TowerCount} torres");
            }

            var tower = await _context.Towers.FirstOrDefaultAsync(t => t.TowerId == userCreateDTO.TowerId);

            if (null == tower)
            {
                throw new ArgumentException($"Torre {userCreateDTO.TowerId} não encontrada");
            }

            if (userCreateDTO.FloorId > tower.FloorCount)
            {
                throw new InconsistentDataException($"Número de andar incorreto. A torre {tower.Name} possui {tower.FloorCount} andares");
            }

            //Regra para 1 apartamento por vaga.
            var usersParkingSpaceNumber = (from usr in context.Users
                         join login in context.Logins on usr.UserId equals login.UserId
                         where login.Enabled == true 
                         && usr.CondominiumId == userCreateDTO.CondominiumId
                         && usr.ParkingSpaceNumber == userCreateDTO.ParkingSpaceNumber
                         && usr.Apartment != userCreateDTO.Apartment
                         select new
                         {
                             User = usr
                         }).ToList();

            if (null != usersParkingSpaceNumber && usersParkingSpaceNumber.Count > 0)
            {
                Log.Debug($"ParkingSpaceNumber no available trying to add new resident. Apartment: {userCreateDTO.Apartment}, ParkingSpaceNumber: {userCreateDTO.ParkingSpaceNumber}");
                foreach (var item in usersParkingSpaceNumber)
                {
                    Log.Debug($"Apartment: {item.User.Apartment} of {item.User.Name} using the parkingspacenumber");
                }

                throw new InconsistentDataException($"Número de vaga especificada já está em uso para outro apartamento. Entre em contato com o administrador para mais informações.");
            }
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
