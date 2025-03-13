using System.Text.Json.Serialization;

namespace SmartCondoApi.Models;

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

    public string Phone1 { get; set; }

    public string? Phone2 { get; set; }

    // Chave estrangeira para UserType
    public int UserTypeId { get; set; }

    // Propriedade de navegação para UserType
    public UserType UserType { get; set; }
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
    public int? ParkingSpaceNumber { get; set; }

    [JsonIgnore]
    public ICollection<Vehicle> Vehicles { get; set; }

    // Relacionamento com Message (Sender)
    [JsonIgnore]
    public ICollection<Message> SentMessages { get; set; }

    // Relacionamento com Message (Recipient)
    [JsonIgnore]
    public ICollection<Message> ReceivedMessages { get; set; }
}
