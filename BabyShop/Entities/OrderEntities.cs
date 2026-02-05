public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }
    //public User User { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;

    public decimal TotalPrice { get; set; }

    //public OrderStatus Status { get; set; } = OrderStatus.Pending;

    //public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public string ShippingAddress { get; set; }

    public string ReceiverName { get; set; }
    public string ReceiverPhone { get; set; }

    public DateTime? ShippedDate { get; set; }

    //public ICollection<OrderItem> OrderItems { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
