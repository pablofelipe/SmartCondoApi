namespace SmartCondoApi.Models.Permissions
{
    public class UserTypePermission
    {
        public int Id { get; set; }
        public string UserTypeName { get; set; }
        public bool CanSendToIndividuals { get; set; }
        public bool CanSendToGroups { get; set; }
        public string[] AllowedRecipientTypes { get; set; }
        public bool CanReceiveMessages { get; set; }
    }
}
