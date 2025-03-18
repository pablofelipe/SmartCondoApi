namespace SmartCondoApi.Dto
{
    public class UserUpdateDTO
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public int? CondominiumId { get; set; }
        public int? TowerNumber { get; set; }
        public int? FloorId { get; set; }
        public int? Apartment { get; set; }
        public LoginUpdateDTO? Login { get; set; }
    }
}
