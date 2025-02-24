using System.Text.Json.Serialization;

namespace SmartCondoApi.Models;

public class Login
{
    public int LoginId { get; set; } // Chave primária
    public string Email { get; set; }
    public string Password { get; set; } // Senha com hash
    public DateOnly Expiration { get; set; }
    public bool Enabled { get; set; }

    [JsonIgnore]
    public User? User { get; set; }
}
