using SmartCondoApi.Dto;

namespace SmartCondoApi.Models.Permissions
{
    public static class RolePermissions
    {
        public static Dictionary<string, UserPermissionsDTO> GetPermissions()
        {
            var rawPermissions = RawPermissions;

            foreach (var permission in rawPermissions)
            {
                var userPermission = permission.Value;

                userPermission.CanSendMessages = userPermission.CanSendToIndividuals || userPermission.CanSendToGroups;
                userPermission.CanRegisterVehicles = userPermission.CanRegisterUsers;
            }

            return rawPermissions;
        }

        private static readonly Dictionary<string, UserPermissionsDTO> RawPermissions = new()
        {
            ["SystemAdministrator"] = new UserPermissionsDTO
            {
                CanSendToIndividuals = true,
                CanSendToGroups = true,
                CanReceiveMessages = true,
                CanRegisterUsers = true,
                AllowedRecipientTypes = ["SystemAdministrator", "CondominiumAdministrator", "Resident", "Janitor"]
            },
            ["CondominiumAdministrator"] = new UserPermissionsDTO
            {
                CanSendToIndividuals = true,
                CanSendToGroups = true,
                CanReceiveMessages = true,
                CanRegisterUsers = true,
                AllowedRecipientTypes = ["CondominiumAdministrator", "ResidentCommitteeMember", "Resident", "Janitor"]
            },
            ["Resident"] = new UserPermissionsDTO
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = true,
                CanRegisterUsers = false,
                AllowedRecipientTypes = ["SystemAdministrator", "CondominiumAdministrator"]
            },
            ["Janitor"] = new UserPermissionsDTO
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = true,
                CanRegisterUsers = false,
                AllowedRecipientTypes = ["SystemAdministrator", "CondominiumAdministrator", "Resident"]
            },
            ["Doorman"] = new UserPermissionsDTO
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = true,
                CanRegisterUsers = false,
                AllowedRecipientTypes = ["SystemAdministrator", "CondominiumAdministrator", "Resident"]
            },
            ["Cleaner"] = new UserPermissionsDTO
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = true,
                CanRegisterUsers = false,
                AllowedRecipientTypes = ["CondominiumAdministrator", "Janitor", "CleaningManager"]
            },
            ["Security"] = new UserPermissionsDTO
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = false,
                CanRegisterUsers = false,
                AllowedRecipientTypes = ["CondominiumAdministrator", "Janitor"]
            },
            ["ServiceProvider"] = new UserPermissionsDTO
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = true,
                CanRegisterUsers = false,
                AllowedRecipientTypes = ["CondominiumAdministrator", "Resident"]
            },
            ["ExternalProvider"] = new UserPermissionsDTO
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = false,
                CanRegisterUsers = false,
                AllowedRecipientTypes = ["Resident"]
            },
            ["DeliveryPerson"] = new UserPermissionsDTO
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = true,
                CanRegisterUsers = false,
                AllowedRecipientTypes = ["Resident"]
            },
            ["CleaningManager"] = new UserPermissionsDTO
            {
                CanSendToIndividuals = true,
                CanSendToGroups = false,
                CanReceiveMessages = true,
                CanRegisterUsers = false,
                AllowedRecipientTypes = ["CondominiumAdministrator", "Janitor", "Cleaner"]
            },
            ["Visitor"] = new UserPermissionsDTO
            {
                CanSendToIndividuals = false,
                CanSendToGroups = false,
                CanReceiveMessages = false,
                CanRegisterUsers = false,
                AllowedRecipientTypes = []
            },
            ["ResidentCommitteeMember"] = new UserPermissionsDTO
            {
                CanSendToIndividuals = true,
                CanSendToGroups = true,
                CanReceiveMessages = true,
                CanRegisterUsers = true,
                AllowedRecipientTypes = ["CondominiumAdministrator", "ResidentCommitteeMember", "Resident", "Janitor"]
            },
        };
    }
}
