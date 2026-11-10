using System.Reflection;
using System.Text;
using CoffeeNChill.ApiGateway.Models;
using CoffeeNChill.ApiGateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace CoffeeNChill.ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register application services
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();

            // Register user storage, token and password services
            builder.Services.AddSingleton<UserStorageService>();
            builder.Services.AddSingleton<TokenService>();

            builder.Services.AddScoped<
                IPasswordHasher<User>,
                PasswordHasher<User>>();

            // Read JWT settings from configuration
            string jwtKey = builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT key is missing. Please configure User Secrets.");

            string jwtIssuer = builder.Configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "JWT issuer is missing.");

            string jwtAudience = builder.Configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "JWT audience is missing.");

            // Check that the JWT key is long enough
            if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
            {
                throw new InvalidOperationException(
                    "JWT key must be at least 32 bytes long.");
            }

            // Configure JWT authentication
            builder.Services.AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = jwtIssuer,

                            ValidateAudience = true,
                            ValidAudience = jwtAudience,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(jwtKey)),

                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.FromMinutes(1)
                        };
                });

            // Configure Swagger
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CoffeeNChill API Gateway",
                    Version = "v1",
                    Description = "API Gateway for CoffeeNChill services."
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token here."
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] =
                        new List<string>()
                });

                var xmlFile =
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

                var xmlPath = Path.Combine(
                    AppContext.BaseDirectory,
                    xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            var app = builder.Build();

            // Enable Swagger in development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // HTTPS redirection disabled for local testing
            // app.UseHttpsRedirection();

            // Enable authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map controller endpoints
            app.MapControllers();

            app.Run();
        }
    }
}
