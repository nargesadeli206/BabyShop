using BabyShop.Core.Entities.Base;

namespace BabyShop.Core.Entities
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;

        // --- روابط (Navigations) مچ شده با AppDbContext ---
        public virtual Order? Order { get; set; }     // حل ارور Order
        public virtual Product? Product { get; set; } // حل ارور Product

        public OrderItem() { }

        public OrderItem(int productId, string productName, int quantity, decimal unitPrice)
        {
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
    }
}