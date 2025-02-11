namespace SmartCondoApi.Models;

public partial class Login
{
    public int LoginId { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateOnly Expiration { get; set; }

    public bool? Enabled { get; set; }

    public virtual User UserLogin { get; set; } = null!;
}
