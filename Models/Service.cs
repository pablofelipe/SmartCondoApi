namespace SmartCondoApi.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public DateOnly ServiceDate { get; set; }

    public string Description { get; set; } = null!;

    public byte[]? ServiceData { get; set; }

    public int UserLoginId { get; set; }

    public int ServiceTypeId { get; set; }

    public ServiceType ServiceType { get; set; } = null!;

    public User? User { get; set; } = null;
}
