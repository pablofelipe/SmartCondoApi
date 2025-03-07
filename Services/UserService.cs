using Microsoft.EntityFrameworkCore;
using SmartCondoApi.Exceptions;
using SmartCondoApi.Infra;
using SmartCondoApi.Models;

namespace SmartCondoApi.Services
{
    public class UserService(SmartCondoContext context)
    {
        private readonly SmartCondoContext _context = context;

        // Adicionar um usuário (assíncrono)
        public async Task AddUserAsync(User user)
        {
            // Valida o CPF
            if (!ValidateCPF(user.PersonalTaxID))
            {
                throw new ArgumentException("CPF inválido.");
            }

            // Encripta o password
            user.Login.Password = new SecurityHandler().EncryptText(user.Login.Password);

            // Adiciona o usuário ao banco de dados
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        private static bool ValidateCPF(string personalTaxID)
        {
            return new IndividualTaxpayerRegistryHandler().Verify(personalTaxID);
        }

        // Atualizar um usuário (assíncrono)
        public async Task UpdateUserAsync(User updatedUser)
        {
            if (null == updatedUser)
                throw new InvalidCredentialsException("Dados de usuario são obrigatórios.");

            // Valida o CPF
            if (!ValidateCPF(updatedUser.PersonalTaxID))
                throw new InvalidPersonalTaxIDException("CPF inválido.");

            // Busca o usuário existente no banco de dados
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == updatedUser.UserId);

            if (null == existingUser)
                throw new UserNotFoundException("Usuário não encontrado.");

            // Atualiza apenas as propriedades escalares
            existingUser.Name = updatedUser.Name;
            existingUser.Address = updatedUser.Address;
            existingUser.Type = updatedUser.Type;
            existingUser.PersonalTaxID = updatedUser.PersonalTaxID;
            existingUser.LoginId = updatedUser.LoginId;
            existingUser.CondominiumId = updatedUser.CondominiumId;
            existingUser.TowerId = updatedUser.TowerId;
            existingUser.FloorId = updatedUser.FloorId;
            existingUser.Apartment = updatedUser.Apartment;

            // Encripta o password se ele foi alterado
            if (!string.IsNullOrEmpty(updatedUser.Login.Password))
            {
                existingUser.Login.Password = new SecurityHandler().EncryptText(updatedUser.Login.Password);
            }

            await _context.SaveChangesAsync();
        }
    }
}
