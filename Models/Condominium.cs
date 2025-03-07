namespace SmartCondoApi.Models
{
    public class Condominium
    {
        public int CondominiumId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int TowerCount { get; set; }
        public bool Enabled { get; set; }

        public ICollection<Tower> Towers { get; set; }

        public ICollection<User> Users { get; set; }

        public ICollection<Service> Services { get; set; }

        public ICollection<Message> Messages { get; set; }

    }
}
