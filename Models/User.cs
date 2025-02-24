namespace SmartCondoApi.Models;

public class User
{
    public int UserId { get; set; } // Chave primária
    public string Name { get; set; }
    public string Address { get; set; }
    public int Type { get; set; } // 1 = Admin, 2 = Standard

    // Chave estrangeira para Login
    public int LoginId { get; set; }
    public Login Login { get; set; } // Propriedade de navegação para Login

    public ICollection<Service>? Services { get; set; } = null;
}
