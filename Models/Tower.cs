namespace SmartCondoApi.Models
{
    public partial class Tower
    {
        public int TowerId { get; set; }
        public string Name { get; set; }
        public int CondominiumId { get; set; }
        public Condominium Condominium { get; set; }
        public int FloorCount { get; set; }

        public ICollection<User> Users { get; set; }

        public ICollection<Service> Services { get; set; }

        public ICollection<Message> Messages { get; set; }
    }
}
