namespace SmartCondoApi.Dto
{
    public class UserCreateDTO
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int UserTypeId { get; set; }
        public string PersonalTaxId { get; set; }
        public int? CondominiumId { get; set; }
        public int? TowerId { get; set; }
        public int? FloorId { get; set; }
        public int? Apartment { get; set; }

        public int? ParkingSpaceNumber { get; set; }
        public LoginCreateDTO Login { get; set; }
    }
}
