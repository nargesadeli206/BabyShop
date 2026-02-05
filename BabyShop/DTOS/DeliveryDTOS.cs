namespace BabyShop.DTOs
{
    public class CreateDeliveryDto
    {
        public string Address { get; set; } = null!;
        public decimal Price { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
    }
}