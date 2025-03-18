using System.Text.Json.Serialization;

namespace SmartCondoApi.Models;

public class Login
{
    public int LoginId { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateOnly Expiration { get; set; }
    public bool Enabled { get; set; }
    public bool IsEmailConfirmed { get; set; } = false;
    public int UserId { get; set; }
    [JsonIgnore]
    public User User { get; set; }

    [JsonIgnore]
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; }
}
