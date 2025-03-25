namespace SmartCondoApi.Models
{
    public partial class Service
    {
        public int Id { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; }
        public string ProviderName { get; set; }
        public string ProviderContact { get; set; }
        public string Notes { get; set; }

        public int ServiceTypeId { get; set; }
        public ServiceType ServiceType { get; set; }

        public int CondominiumId { get; set; }
        public Condominium Condominium { get; set; }

        public int? TowerId { get; set; }
        public Tower Tower { get; set; }

        public int? FloorId { get; set; }
    }
}