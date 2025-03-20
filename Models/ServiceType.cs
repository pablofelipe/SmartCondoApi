namespace SmartCondoApi.Models;

public partial class ServiceType
{
    public int Id { get; set; }

    public string Name { get; set; } = null;

    public string Description { get; set; } = null!;

    public ICollection<Service> Services { get; set; }
}
