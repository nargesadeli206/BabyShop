using System.ComponentModel.DataAnnotations;

namespace BabyShop.Entities
{
    public class Delivery
    {
        public int Id { get; set; }  

        [Required]
        public string Address { get; set; } = null!;

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Postal code must be 10 digits")]
        public string PostalCode { get; set; } = null!;
    }
}