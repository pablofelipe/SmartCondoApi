namespace SmartCondoApi.Models
{
    public class UserType
    {
        public int UserTypeId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; } = null!;
    }
}
