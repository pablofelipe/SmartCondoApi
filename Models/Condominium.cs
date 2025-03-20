namespace SmartCondoApi.Models
{
    public class Condominium
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int TowerCount { get; set; }
        public bool Enabled { get; set; }
        public int MaxUsers { get; set; }

        public ICollection<Tower> Towers { get; set; }

        public ICollection<UserProfile> Users { get; set; }

        public ICollection<Service> Services { get; set; }

        public ICollection<Message> Messages { get; set; }

    }
}
