namespace SmartCondoApi.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public DateOnly ServiceDate { get; set; }

    public string Description { get; set; } = null!;

    public byte[]? ServiceData { get; set; }

    public int UserLoginId { get; set; }

    public int ServiceTypeId { get; set; }

    public virtual ServiceType ServiceType { get; set; } = null!;

    public virtual User UserLogin { get; set; } = null!;
}
