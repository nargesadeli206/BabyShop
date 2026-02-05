public class CreateOrderDto
{
    public int UserId { get; set; }

    public string ShippingAddress { get; set; }

    public List<CreateOrderItemDto> Items { get; set; }
}