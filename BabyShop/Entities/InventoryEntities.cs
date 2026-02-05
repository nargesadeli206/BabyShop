namespace BabyShop.Entities
{
    public class Inventory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Count { get; set; } = 0;  
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public Product? Product { get; set; }
    }
}