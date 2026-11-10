namespace CoffeeNChill.ApiGateway.Models
{
    // This class will describe the information needed to register.
    public class RegisterDto
    {
        // The user's full name.
        public string FullName { get; set; } = "";

        // The user's email address.
        public string Email { get; set; } = "";

        // The password chosen by the user.
        public string Password { get; set; } = "";
    }
}
