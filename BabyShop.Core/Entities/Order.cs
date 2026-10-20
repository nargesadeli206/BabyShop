using BabyShop.Core.Entities.Base;
using System;
using System.Collections.Generic;

namespace BabyShop.Core.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal TotalAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingCost { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CustomerNote { get; set; }
        public string? AdminNote { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? UserName { get; set; }

        // --- روابط (Navigations) مچ شده با AppDbContext ---
        public virtual User? User { get; set; }
        public virtual List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public virtual Payment? Payment { get; set; }   // حل ارور Payment
        public virtual Delivery? Delivery { get; set; } // حل ارور Delivery

        public Order() { }

        public Order(int userId, string address, string phone, string email)
        {
            UserId = userId;
            ShippingAddress = address;
            PhoneNumber = phone;
            Email = email;
            Status = "Pending";
            CreatedAt = DateTime.Now;
            OrderNumber = "ORD-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }

        public void AddItem(int productId, string productName, int quantity, decimal unitPrice)
        {
            Items.Add(new OrderItem(productId, productName, quantity, unitPrice));
            CalculateTotals();
        }

        public void AddItem(OrderItem item)
        {
            Items.Add(item);
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            SubTotal = 0;
            foreach (var item in Items) SubTotal += (item.UnitPrice * item.Quantity);
            Tax = Math.Round(SubTotal * 0.08m);
            TotalAmount = SubTotal + Tax + ShippingCost - Discount;
        }

        public void MarkAsPaid() { Status = "Paid"; PaidAt = DateTime.Now; }
        public void MarkAsShipped() { Status = "Shipped"; ShippedAt = DateTime.Now; }
        public void MarkAsDelivered() { Status = "Delivered"; DeliveredAt = DateTime.Now; }
        public void MarkAsRefunded() { Status = "Refunded"; }
        public void Cancel(string reason) { Status = "Cancelled"; CancellationReason = reason; }
    }
}