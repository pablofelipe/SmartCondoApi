using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SmartCondoApi.Models.Permissions;

namespace SmartCondoApi.Services.Permission
{
    public class PermissionService(RoleManager<IdentityRole<long>> roleManager, IMemoryCache cache)
    {
        private readonly RoleManager<IdentityRole<long>> _roleManager = roleManager;
        private readonly IMemoryCache _cache = cache;
        private const string PermissionsCacheKey = "UserTypePermissions";

        public async Task<Dictionary<string, UserTypePermission>> GetPermissionsAsync()
        {
            return await _cache.GetOrCreateAsync(PermissionsCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1); // Cache por 1 hora
                return await LoadPermissionsFromDbAsync();
            });
        }

        private async Task<Dictionary<string, UserTypePermission>> LoadPermissionsFromDbAsync()
        {
            var permissions = new Dictionary<string, UserTypePermission>();

            // Carrega todas as roles do sistema
            var roles = await _roleManager.Roles.ToListAsync();

            foreach (var role in roles)
            {
                // Obtém todas as claims da role
                var claims = await _roleManager.GetClaimsAsync(role);

                var permission = new UserTypePermission
                {
                    CanSendToIndividuals = claims.Any(c => c.Type == "Permission" && c.Value == "CanSendToIndividuals"),
                    CanSendToGroups = claims.Any(c => c.Type == "Permission" && c.Value == "CanSendToGroups"),
                    CanReceiveMessages = claims.Any(c => c.Type == "Permission" && c.Value == "CanReceiveMessages"),
                    AllowedRecipientTypes = claims
                        .Where(c => c.Type == "AllowedRecipient")
                        .Select(c => c.Value)
                        .ToArray()
                };

                permissions.Add(role.Name, permission);
            }

            return permissions;
        }
    }
}
