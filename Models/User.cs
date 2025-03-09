
using System.Text.Json.Serialization;

namespace SmartCondoApi.Models;

public enum UserType
{
    SysAdmin,
    CondoAdmin,
    ServiceProvider,
    Resident
}

public class User
{
    public User()
    {
        Vehicles = [];
        SentMessages = [];
        ReceivedMessages = [];
    }

    public int UserId { get; set; } // Chave primária
    public string Name { get; set; }
    public string Address { get; set; }
    
    public UserType Type { get; set; }
    //CPF
    public string PersonalTaxID { get; set; }

    [JsonIgnore]
    public Login? Login { get; set; }

    public int? CondominiumId { get; set; }
    [JsonIgnore]
    public Condominium? Condominium { get; set; }

    public int? TowerId { get; set; }
    [JsonIgnore]
    public Tower? Tower { get; set; }

    public int? FloorId { get; set; }
    public int? Apartment { get; set; }

    [JsonIgnore]
    public ICollection<Vehicle> Vehicles { get; set; }

    // Relacionamento com Message (Sender)
    [JsonIgnore]
    public ICollection<Message> SentMessages { get; set; }

    // Relacionamento com Message (Recipient)
    [JsonIgnore]
    public ICollection<Message> ReceivedMessages { get; set; }
}
