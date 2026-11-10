namespace CoffeeNChill.ApiGateway.Models
{
    // This class describes the information stored for each menu item.
    public class MenuItem
    {
        // The category the menu item belongs to.
        public string Category { get; set; } = "";

        // The unique ID of the menu item.
        public string Id { get; set; } = "";

        // The name of the menu item.
        public string Name { get; set; } = "";

        // A description of the menu item.
        public string Description { get; set; } = "";

        // The price of the menu item.
        public double Price { get; set; }

        // Shows whether the menu item is available.
        public bool IsAvailable { get; set; }
    }
}
