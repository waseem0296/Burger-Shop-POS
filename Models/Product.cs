// ============================================================
// FILE: Models/Product.cs
// Represents the "Products" table in your SQL database.
// ============================================================

namespace BurgerShopPOS.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ItemName { get; set; } = string.Empty;

        // Category: "Burgers", "Sides", "Drinks"
        public string Category { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateTime ExpiryDate { get; set; }

        // Navigation property
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
