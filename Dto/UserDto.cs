namespace SmartCondoApi.dto
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int Type { get; set; }
        public int LoginId { get; set; }
        public LoginDto Login { get; set; }
    }
}
