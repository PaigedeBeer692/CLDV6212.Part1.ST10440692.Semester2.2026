using System;
using System.Collections.Generic;

namespace CoffeeNChill.Functions.Models
{
    // Represents an order placed by a CoffeeNChill customer.
    // This class is used to store the order details received by the system.
    public class Order
    {
        // A unique reference used to identify the order.
        public string OrderId { get; set; } = string.Empty;

        // The name of the customer who placed the order.
        public string CustomerName { get; set; } = string.Empty;

        // The SKUs identify the menu items selected for this order.
        public List<string> SelectedItemSKUs { get; set; } = new List<string>();

        // The total price of all items in the order.
        public decimal TotalPrice { get; set; }

        // The date and time when the order was placed.
        public DateTime OrderTimestamp { get; set; }
    }
}
