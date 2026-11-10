using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.ApiGateway.Models
{
    // This class stores the information for each CoffeeNChill user.
    public class User : ITableEntity
    {
        // Users are stored in the CoffeeNChillUsers table.
        public string PartitionKey { get; set; } = "Users";

        // Each user's email address is used as their unique ID.
        public string RowKey { get; set; } = "";

        // The user's full name.
        public string FullName { get; set; } = "";

        // The user's email address.
        public string Email { get; set; } = "";

        // The securely hashed password.
        public string PasswordHash { get; set; } = "";

        // The user's access role.
        public string Role { get; set; } = "Customer";

        // The date and time the account was created.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Azure Table Storage requires these properties.
        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }
}
