namespace SmartCondoApi.dto
{
    public class LoginDto
    {
        public int LoginId { get; set; }
        public string Email { get; set; }
        public DateOnly Expiration { get; set; }
        public bool Enabled { get; set; }
    }
}
