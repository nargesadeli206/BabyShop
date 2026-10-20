using BabyShop.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BabyShop.Core.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id);
    Task<IReadOnlyList<Payment>> GetAllAsync();
    Task<Payment> AddAsync(Payment entity);
    Task UpdateAsync(Payment entity);
    Task DeleteAsync(Payment entity);

    Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
    Task<Payment?> GetPaymentByAuthorityAsync(string authority);
}