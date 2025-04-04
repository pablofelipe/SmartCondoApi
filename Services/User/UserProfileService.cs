﻿using Microsoft.EntityFrameworkCore;
using Serilog;
using SmartCondoApi.Dto;
using SmartCondoApi.Exceptions;
using SmartCondoApi.Infra;
using SmartCondoApi.Models;

namespace SmartCondoApi.Services.User
{
    public class UserProfileService(IUserProfileServiceDependencies _dependencies) : IUserProfileService
    {
        public async Task<UserProfileResponseDTO> Add(UserProfileCreateDTO userCreateDTO)
        {
            // Valida o CPF/CNPJ
            if (!ValidateRegistrationNumber(userCreateDTO.RegistrationNumber))
            {
                throw new InvalidRegistrationNumberIDException("CPF/CNPJ inválido");
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

            if (context.UserProfiles.Any(u => u.RegistrationNumber == userCreateDTO.RegistrationNumber))
            {
                throw new UserAlreadyExistsException($"CPF {userCreateDTO.RegistrationNumber} já cadastrado");
            }

            var userTypes = await _dependencies.Context.UserTypes.FirstOrDefaultAsync(ut => ut.Id == userCreateDTO.UserTypeId);

            if (null == userTypes)
            {
                throw new ArgumentException($"Tipo de usuário {userCreateDTO.UserTypeId} não encontrado");
            }

            var condo = await context.Condominiums.FirstOrDefaultAsync(c => c.Id == userCreateDTO.CondominiumId);

            if (null == condo)
            {
                if (IsSystemAdmin(userTypes.Name) == false)
                    throw new ArgumentException($"Condominio {userCreateDTO.CondominiumId} não encontrado");

                SystemAdministrationValidations(userCreateDTO, userTypes, condo);
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

                if (count > condo.MaxUsers - 1)
                {
                    throw new UsersExceedException("O número máximo de usuários permitidos para este condomínio foi atingido. Entre em contato com o administrador para mais informações.");
                }

                await ResidentValidations(userCreateDTO, userTypes, condo);

                NonResidentValidations(userCreateDTO, userTypes);
            }

            var user = new Models.User
            {
                UserName = userCreateDTO.User.Email,
                Email = userCreateDTO.User.Email,
                EmailConfirmed = false,
                LockoutEnabled = true,
                TwoFactorEnabled = true
            };

            if (Environment.GetEnvironmentVariable("DISABLE_ENCRYPTION") != "true")
            {
                var cryptoService = _dependencies.CryptoService;

                userCreateDTO.User.Password = cryptoService.DecryptData(userCreateDTO.User.KeyId, userCreateDTO.User.Password);
            }

            user.PasswordHash = _dependencies.UserManager.PasswordHasher.HashPassword(user, userCreateDTO.User.Password);

            var userProfile = new UserProfile
            {
                Name = userCreateDTO.Name,
                Address = userCreateDTO.Address,
                Phone1 = userCreateDTO.Phone1,
                Phone2 = userCreateDTO.Phone2,
                UserTypeId = userCreateDTO.UserTypeId,
                RegistrationNumber = userCreateDTO.RegistrationNumber,
                CondominiumId = userCreateDTO.CondominiumId,
                TowerId = userCreateDTO.TowerId,
                FloorNumber = userCreateDTO.FloorId,
                Apartment = userCreateDTO.Apartment,
                ParkingSpaceNumber = userCreateDTO.ParkingSpaceNumber,
                User = user
            };


            await context.UserProfiles.AddAsync(userProfile);
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();


            await _dependencies.UserManager.UpdateSecurityStampAsync(user);
            await _dependencies.UserManager.UpdateNormalizedEmailAsync(user);
            await _dependencies.UserManager.UpdateNormalizedUserNameAsync(user);
            var token = await _dependencies.UserManager.GenerateEmailConfirmationTokenAsync(user);

            //await dependencies.UserManager.AddToRoleAsync(user, userTypes.Name);

            //var roles = await dependencies.UserManager.GetRolesAsync(user);
            //foreach (var role in roles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, role));
            //}

            return new UserProfileResponseDTO()
            {
                Id = user.Id,
                Name = userCreateDTO.Name,
                Address = userCreateDTO.Address,
                UserTypeId = userCreateDTO.UserTypeId,
                RegistrationNumber = userCreateDTO.RegistrationNumber,
                CondominiumId = userCreateDTO.CondominiumId,
                TowerId = userCreateDTO.TowerId,
                FloorId = userCreateDTO.FloorId,
                Apartment = userCreateDTO.Apartment,
                ParkingSpaceNumber = userCreateDTO.ParkingSpaceNumber,
                Message = "Usuário registrado. Verifique seu e-mail para confirmar o cadastro.",
                Token = token
            };
        }

        private static void NonResidentValidations(UserProfileCreateDTO userCreateDTO, UserType currentUserType)
        {

            if (IsResident(currentUserType.Name))
                return;

            userCreateDTO.FloorId = null;
            userCreateDTO.Apartment = null;
            userCreateDTO.ParkingSpaceNumber = null;
            userCreateDTO.TowerId = null;
        }

        private static void SystemAdministrationValidations(UserProfileCreateDTO userCreateDTO, UserType currentUserType, Models.Condominium condo)
        {

            if (!IsSystemAdmin(currentUserType.Name))
                return;

            userCreateDTO.FloorId = null;
            userCreateDTO.Apartment = null;
            userCreateDTO.ParkingSpaceNumber = null;
            userCreateDTO.TowerId = null;
        }

        private static bool IsSystemAdmin(string userTypeName)
        {
            return string.Compare(userTypeName, "SystemAdministrator", StringComparison.OrdinalIgnoreCase) == 0;
        }

        private static bool IsResident(string userTypeName)
        {
            return string.Compare(userTypeName, "Resident", StringComparison.OrdinalIgnoreCase) == 0;
        }

        private async Task ResidentValidations(UserProfileCreateDTO userCreateDTO, UserType currentUserType, Models.Condominium condo)
        {

            if (!IsResident(currentUserType.Name))
                return;

            if (userCreateDTO.Apartment <= 0)
            {
                throw new InconsistentDataException($"Número de apartamento incorreto.");
            }

            var context = _dependencies.Context;

            var tower = await context.Towers.FirstOrDefaultAsync(t => t.Id == userCreateDTO.TowerId && t.CondominiumId == condo.Id);

            if (null == tower)
            {
                throw new ArgumentException($"Torre {userCreateDTO.TowerId} não encontrada");
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

        private static bool ValidateRegistrationNumber(string registrationNumber)
        {
            return new RegistrationNumberValidator().Verify(registrationNumber);
        }

        public async Task<UserProfileResponseDTO> Update(long userId, UserProfileUpdateDTO userUpdateDTO)
        {
            if (null == userUpdateDTO)
            {
                throw new InvalidCredentialsException("Dados de usuario são obrigatórios.");
            }

            var context = _dependencies.Context;

            // Busca o usuário existente no banco de dados
            var userProfile = await context.UserProfiles
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (null == userProfile)
                throw new UserNotFoundException("Usuário não encontrado.");

            if (userUpdateDTO.Name != null) userProfile.Name = userUpdateDTO.Name;
            if (userUpdateDTO.Address != null) userProfile.Address = userUpdateDTO.Address;

            if (userUpdateDTO.CondominiumId.HasValue) userProfile.CondominiumId = userUpdateDTO.CondominiumId.Value;
            if (userUpdateDTO.TowerId.HasValue) userProfile.TowerId = userUpdateDTO.TowerId.Value;
            if (userUpdateDTO.FloorId.HasValue) userProfile.FloorNumber = userUpdateDTO.FloorId.Value;
            if (userUpdateDTO.Apartment.HasValue) userProfile.Apartment = userUpdateDTO.Apartment.Value;

            // Atualiza os dados do Login, se fornecido
            if (userUpdateDTO.User != null)
            {
                //não altera o email!
                //if (userUpdateDTO.User.Email != null) user.Email = userUpdateDTO.User.Email;
                //não é o caminho para desabilitar
                //if (userUpdateDTO.User.Enabled.HasValue) user.Enabled = userUpdateDTO.User.Enabled.Value;

                if (!string.IsNullOrEmpty(userUpdateDTO.User.Password))
                {
                    var user = await _dependencies.UserManager.FindByIdAsync(userId.ToString());

                    if (null != user)
                    {
                        if (Environment.GetEnvironmentVariable("DISABLE_ENCRYPTION") != "true")
                        {
                            var cryptoService = _dependencies.CryptoService;

                            userUpdateDTO.User.Password = cryptoService.DecryptData(userUpdateDTO.User.KeyId, userUpdateDTO.User.Password);
                        }

                        user.PasswordHash = _dependencies.UserManager.PasswordHasher.HashPassword(user, userUpdateDTO.User.Password);
                    }
                }
            }

            await context.SaveChangesAsync();

            return new UserProfileResponseDTO()
            {
                Name = userProfile.Name,
                Address = userProfile.Address,
                RegistrationNumber = userProfile.RegistrationNumber,
                CondominiumId = userProfile.CondominiumId,
                TowerId = userProfile.TowerId,
                FloorId = userProfile.FloorNumber,
                Apartment = userProfile.Apartment,
                Message = "Usuário atualizado.",
            };
        }

        public async Task<IEnumerable<UserProfile>> Get()
        {
            return await _dependencies.Context.UserProfiles.ToListAsync();
        }

        public async Task<UserProfileEditDTO> Get(long id)
        {
            var userProfile = await _dependencies.Context.UserProfiles.FindAsync(id);
            if (userProfile == null)
            {
                throw new UserNotFoundException("Perfil de usuário não encontrado.");
            }

            var user = await _dependencies.UserManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                throw new UserNotFoundException("Usuário não encontrado.");
            }

            var dto = new UserProfileEditDTO
            {
                Id = userProfile.Id,
                Name = userProfile.Name,
                Address = userProfile.Address,
                Phone1 = userProfile.Phone1,
                Phone2 = userProfile.Phone2,
                UserTypeId = userProfile.UserTypeId,
                RegistrationNumber = userProfile.RegistrationNumber,
                CondominiumId = userProfile.CondominiumId,
                TowerId = userProfile.TowerId,
                FloorId = userProfile.FloorNumber,
                Apartment = userProfile.Apartment,
                ParkingSpaceNumber = userProfile.ParkingSpaceNumber,
                Email = user.Email ?? string.Empty,
                Enabled = user.Enabled,
                PasswordLength = 8 // Não sabemos o tamanho real, será apenas visual
            };

            return dto;
        }

        public async Task Delete(long id)
        {
            if (id < 1)
            {
                throw new InconsistentDataException($"Numero do id {id} incorreto.");
            }

            var userProfile = await _dependencies.Context.UserProfiles.FindAsync(id);
            if (userProfile == null)
            {
                throw new UserNotFoundException("Usuário não encontrado.");
            }

            if (null != userProfile.User)
            {
                userProfile.User.Enabled = false;
            }
            else
            {
                _dependencies.Context.UserProfiles.Remove(userProfile);
            }

            await _dependencies.Context.SaveChangesAsync();
        }
    }
}
