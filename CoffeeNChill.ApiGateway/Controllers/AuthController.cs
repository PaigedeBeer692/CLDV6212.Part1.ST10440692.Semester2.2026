using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CoffeeNChill.ApiGateway.Models;
using CoffeeNChill.ApiGateway.Services;

namespace CoffeeNChill.ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserStorageService _userStorage;
        private readonly TokenService _tokenService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthController(
            UserStorageService userStorage,
            TokenService tokenService,
            IPasswordHasher<User> passwordHasher)
        {
            _userStorage = userStorage;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        // This will register a new customer account
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Please complete all fields.");
            }

            if (!request.Email.Contains("@"))
            {
                return BadRequest("Please enter a valid email address.");
            }

            if (request.Password.Length < 8)
            {
                return BadRequest("Password must contain at least 8 characters.");
            }

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Role = "Customer"
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            bool created = await _userStorage.CreateUserAsync(user);

            if (!created)
            {
                return Conflict("An account with this email already exists.");
            }

            return Ok(new
            {
                message = "Account registered successfully.",
                fullName = user.FullName,
                email = user.Email,
                role = user.Role
            });
        }

        // This will login an existing customer
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Please enter your email and password.");
            }

            var user = await _userStorage.GetUserByEmailAsync(
                request.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid email or password.");
            }

            string token = _tokenService.GenerateToken(user);

            return Ok(new
            {
                message = "Login successful.",
                token = token,
                fullName = user.FullName,
                email = user.Email,
                role = user.Role
            });
        }
    }
}
