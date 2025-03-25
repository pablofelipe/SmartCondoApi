namespace SmartCondoApi.Dto
{
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public UserProfileDTO User { get; set; }
    }

    public class UserProfileDTO
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Role { get; set; } // Ex: "CondominiumAdministrator"

        public int CondominiumId { get; set; }
        public int? TowerId { get; set; }
        public int? FloorId { get; set; }
        public int? Apartment { get; set; }

        public UserPermissionsDTO Permissions { get; set; }
    }

    public class UserPermissionsDTO
    {
        public bool CanSendToIndividuals { get; set; }
        public bool CanSendToGroups { get; set; }
        public bool CanReceiveMessages { get; set; }
        public bool CanRegisterUsers {get; set;}
        public List<string> AllowedRecipientTypes { get; set; } // Tipos de usuários permitidos
    }
}
