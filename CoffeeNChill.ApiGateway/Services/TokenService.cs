using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoffeeNChill.ApiGateway.Models;
using Microsoft.IdentityModel.Tokens;

namespace CoffeeNChill.ApiGateway.Services
{
    // This service will create JWT tokens for authenticated users.
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This will generate a JWT token after a successful login.
        public string GenerateToken(User user)
        {
            //This will read the JWT settings from the application configuration.
            string secretKey =
                _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT secret key is missing.");

            string issuer =
                _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT issuer is missing.");

            string audience =
                _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT audience is missing.");

            // This will convert the secret key into a security key.
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey));

            // This will create the signing credentials.
            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            // This will dd the user's information to the token.
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            // This will create the token with a one-hour expiry time.
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            // This will convert the token into a string.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
