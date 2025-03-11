namespace SmartCondoApi.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public int LoginId { get; set; }
        public Login Login { get; set; }
    }
}
