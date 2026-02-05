public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<Order> GetByIdAsync(int id);
}