
using SmartCondoApi.Dto;

namespace SmartCondoApi.dto
{
    public class UserResponseDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int UserTypeId { get; set; }
        public string PersonalTaxId { get; set; }
        public int? CondominiumId { get; set; }
        public int? TowerId { get; set; }
        public int? FloorId { get; set; }
        public int? Apartment { get; set; }
        public int? ParkingSpaceNumber { get; set; }

        public ICollection<VehicleDTO> Vehicles { get; set; } = new List<VehicleDTO>();
    }
}
