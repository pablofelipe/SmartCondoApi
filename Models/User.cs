namespace SmartCondoApi.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public int Type { get; set; }

    public int LoginId { get; set; }

    public virtual Login Login { get; set; } = null!;

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
