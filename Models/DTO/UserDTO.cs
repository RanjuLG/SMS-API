namespace SMS.Models.DTO
{
    public class UserDTO
    {

        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }  // "Admin" or "Cashier"
    }
}
