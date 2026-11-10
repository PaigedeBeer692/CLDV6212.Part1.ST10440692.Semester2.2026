namespace CoffeeNChill.ApiGateway.Models
{
    // This class describes the information needed to log in.
    public class LoginDto
    {
        // The user's email address.
        public string Email { get; set; } = "";

        // The user's password.
        public string Password { get; set; } = "";
    }
}
