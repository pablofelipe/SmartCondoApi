namespace SmartCondoApi.Dto
{
    public class LoginCreateDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public DateOnly Expiration { get; set; }
        public bool Enabled { get; set; }
    }
}
