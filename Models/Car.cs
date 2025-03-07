namespace SmartCondoApi.Models
{
    public class Car
    {
        public int CarId { get; set; } // PK
        public string LicensePlate { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public bool Enabled { get; set; }

        // Relacionamento com User (dono do carro)
        public int UserId { get; set; } // FK
        public User User { get; set; }

    }
}
