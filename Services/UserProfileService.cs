using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SmartCondoApi.dto;
using SmartCondoApi.Dto;
using SmartCondoApi.Exceptions;
using SmartCondoApi.Infra;
using SmartCondoApi.Models;

namespace SmartCondoApi.Services
{
    public class UserProfileService(IUserProfileDependencies dependencies)
    {
        private readonly IUserProfileDependencies _dependencies = dependencies;

        public async Task<UserProfileResponseDTO> AddUserAsync(UserProfileCreateDTO userCreateDTO)
        {
            // Valida o CPF
            if (!ValidateCPF(userCreateDTO.PersonalTaxId))
            {
                throw new InvalidPersonalTaxIDException("CPF inválido");
            }

            if (null == userCreateDTO.User)
            {
                throw new InvalidCredentialsException("Nenhum login encontrado");
            }

            var userDb = await _dependencies.UserManager.FindByEmailAsync(userCreateDTO.User.Email);

            if (null != userDb)
            {
                throw new LoginAlreadyExistsException($"Login {userCreateDTO.User.Email} já cadastrado");
            }

            var context = _dependencies.Context;

            if (context.UserProfiles.Any(u => u.PersonalTaxID == userCreateDTO.PersonalTaxId))
            {
                throw new UserAlreadyExistsException($"CPF {userCreateDTO.PersonalTaxId} já cadastrado");
            }

            var condo = await context.Condominiums.FirstOrDefaultAsync(c => c.Id == userCreateDTO.CondominiumId);

            if (null == condo)
            {
                if (await SystemAdmin(userCreateDTO) == false)
                    throw new ArgumentException($"Condominio {userCreateDTO.CondominiumId} não encontrado");
            }
            else
            {
                if (!condo.Enabled)
                {
                    throw new CondominiumDisabledException($"Condomínio {condo.Name} desabilitado. Entre em contato com o administrador para mais informações.");
                }

                var count = (from profiles in context.UserProfiles
                             join users in context.Users on profiles.Id equals users.Id
                             where users.Enabled == true && profiles.CondominiumId == userCreateDTO.CondominiumId
                             select new
                             {
                                 User = profiles,
                                 Login = users
                             }).Count();

                if (count > condo.MaxUsers-1)
                {
                    throw new UsersExceedException("O número máximo de usuários permitidos para este condomínio foi atingido. Entre em contato com o administrador para mais informações.");
                }

                await ResidentValidations(userCreateDTO, condo);
            }

            var user = new User
            {
                UserName = userCreateDTO.User.Email,
                Email = userCreateDTO.User.Email,
                EmailConfirmed = false,
                LockoutEnabled = true,
                TwoFactorEnabled = true
            };

            var userProfile = new UserProfile
            {
                Name = userCreateDTO.Name,
                Address = userCreateDTO.Address,
                Phone1 = userCreateDTO.Phone1,
                Phone2 = userCreateDTO.Phone2,
                UserTypeId = userCreateDTO.UserTypeId,
                PersonalTaxID = userCreateDTO.PersonalTaxId,
                CondominiumId = userCreateDTO.CondominiumId,
                TowerNumber = userCreateDTO.TowerNumber,
                FloorId = userCreateDTO.FloorId,
                Apartment = userCreateDTO.Apartment,
                ParkingSpaceNumber = userCreateDTO.ParkingSpaceNumber,
                User = user
            };

            await context.UserProfiles.AddAsync(userProfile);
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            await dependencies.UserManager.UpdateSecurityStampAsync(user);
            await dependencies.UserManager.UpdateNormalizedEmailAsync(user);
            await dependencies.UserManager.UpdateNormalizedUserNameAsync(user);
            var token = await dependencies.UserManager.GenerateEmailConfirmationTokenAsync(user);

            var request = dependencies.HttpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var confirmationLink = $"{baseUrl}/confirm-email?userId={userProfile.Id}&token={token}";

            await dependencies.EmailService.SendEmailAsync(
                userProfile.User.Email,
                "Confirme seu e-mail",
                $"Por favor, confirme seu e-mail clicando neste link: {confirmationLink}"
            );

            return new UserProfileResponseDTO()
            {
                UserId = user.Id,
                Name = userCreateDTO.Name,
                Address = userCreateDTO.Address,
                UserTypeId = userCreateDTO.UserTypeId,
                PersonalTaxId = userCreateDTO.PersonalTaxId,
                CondominiumId = userCreateDTO.CondominiumId,
                TowerNumber = userCreateDTO.TowerNumber,
                FloorId = userCreateDTO.FloorId,
                Apartment = userCreateDTO.Apartment,
                ParkingSpaceNumber = userCreateDTO.ParkingSpaceNumber,
                Message = "Usuário registrado. Verifique seu e-mail para confirmar o cadastro.",
                Token = token
            };
        }

        private async Task<bool> SystemAdmin(UserProfileCreateDTO userCreateDTO)
        {
            var context = _dependencies.Context;

            var userTypes = await context.UserTypes.FirstOrDefaultAsync(ut => ut.Id == userCreateDTO.UserTypeId);

            if (null == userTypes)
            {
                throw new ArgumentException($"Tipo de usuário {userCreateDTO.UserTypeId} não encontrado");
            }

            return string.Compare(userTypes.Name, "SystemAdministrator", StringComparison.OrdinalIgnoreCase) == 0;
        }

        private async Task ResidentValidations(UserProfileCreateDTO userCreateDTO, Condominium condo)
        {
            var context = _dependencies.Context;

            var userTypes = await context.UserTypes.FirstOrDefaultAsync(ut => ut.Id == userCreateDTO.UserTypeId);

            if (null == userTypes)
            {
                throw new ArgumentException($"Tipo de usuário {userCreateDTO.UserTypeId} não encontrado");
            }

            if (string.Compare(userTypes.Name, "Resident", StringComparison.OrdinalIgnoreCase) != 0)
                return;

            if (userCreateDTO.Apartment == 0)
            {
                throw new InconsistentDataException($"Número de apartamento incorreto.");
            }

            var tower = await context.Towers.FirstOrDefaultAsync(t => t.Number == userCreateDTO.TowerNumber && t.CondominiumId == condo.Id);

            if (null == tower)
            {
                throw new ArgumentException($"Torre {userCreateDTO.TowerNumber} não encontrada");
            }

            if (userCreateDTO.FloorId > tower.FloorCount)
            {
                throw new InconsistentDataException($"Número de andar incorreto. A torre {tower.Name} possui {tower.FloorCount} andar(es)");
            }

            //Regra para 1 apartamento por vaga.
            var usersParkingSpaceNumber = (from profiles in context.UserProfiles
                         join users in context.Users on profiles.Id equals users.Id
                         where users.Enabled == true 
                         && profiles.CondominiumId == userCreateDTO.CondominiumId
                         && profiles.ParkingSpaceNumber == userCreateDTO.ParkingSpaceNumber
                         && profiles.Apartment != userCreateDTO.Apartment
                         select new
                         {
                             User = profiles
                         }).ToList();

            if (null != usersParkingSpaceNumber && usersParkingSpaceNumber.Count > 0)
            {
                Log.Debug($"ParkingSpaceNumber no available trying to add new resident. Apartment: {userCreateDTO.Apartment}, ParkingSpaceNumber: {userCreateDTO.ParkingSpaceNumber}");
                foreach (var item in usersParkingSpaceNumber)
                {
                    Log.Debug($"Apartment: {item.User.Apartment} of {item.User.Name} using the parkingspacenumber");
                }

                throw new ParkingSpaceNumberException($"Número de vaga especificada já está em uso para outro apartamento. Entre em contato com o administrador para mais informações.");
            }
        }

        private static bool ValidateCPF(string personalTaxID)
        {
            return new IndividualTaxpayerRegistryHandler().Verify(personalTaxID);
        }

        public async Task<UserProfileResponseDTO> UpdateUserAsync(long userId, UserProfileUpdateDTO userUpdateDTO)
        {
            if (null == userUpdateDTO)
            {
                throw new InvalidCredentialsException("Dados de usuario são obrigatórios.");
            }

            var context = _dependencies.Context;

            // Busca o usuário existente no banco de dados
            var user = await context.UserProfiles
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (null == user)
                throw new UserNotFoundException("Usuário não encontrado.");

            if (userUpdateDTO.Name != null) user.Name = userUpdateDTO.Name;
            if (userUpdateDTO.Address != null) user.Address = userUpdateDTO.Address;

            if (userUpdateDTO.CondominiumId.HasValue) user.CondominiumId = userUpdateDTO.CondominiumId.Value;
            if (userUpdateDTO.TowerNumber.HasValue) user.TowerNumber = userUpdateDTO.TowerNumber.Value;
            if (userUpdateDTO.FloorId.HasValue) user.FloorId = userUpdateDTO.FloorId.Value;
            if (userUpdateDTO.Apartment.HasValue) user.Apartment = userUpdateDTO.Apartment.Value;

            // Atualiza os dados do Login, se fornecido
            if (userUpdateDTO.User != null && user.User != null)
            {
                if (userUpdateDTO.User.Email != null) user.User.Email = userUpdateDTO.User.Email;
                if (userUpdateDTO.User.Password != null) user.User.PasswordHash = userUpdateDTO.User.Password;
                if (userUpdateDTO.User.Enabled.HasValue) user.User.Enabled = userUpdateDTO.User.Enabled.Value;
            }

            await context.SaveChangesAsync();

            return new UserProfileResponseDTO()
            {
                Name = user.Name,
                Address = user.Address,
                PersonalTaxId = user.PersonalTaxID,
                CondominiumId = user.CondominiumId,
                TowerNumber = user.TowerNumber,
                FloorId = user.FloorId,
                Apartment = user.Apartment
            };
        }
    }
}
