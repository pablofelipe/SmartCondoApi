namespace SmartCondoApi.Models
{
    public enum VehicleType
    {
        Car,
        Motorcycle,
        Truck
    }

    public class Vehicle
    {
        public int VehicleId { get; set; } // PK

        public VehicleType Type { get; set; }
        public string LicensePlate { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public bool Enabled { get; set; }

        // Relacionamento com User (dono do veiculo)
        public int UserId { get; set; } // FK
        public User User { get; set; }

    }
}
