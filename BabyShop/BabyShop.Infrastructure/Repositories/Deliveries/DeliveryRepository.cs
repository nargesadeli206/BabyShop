using BabyShop.BabyShop.Core.Entities.Deliveries;
using BabyShop.BabyShop.Core.Interfaces;
using BabyShop.BabyShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BabyShop.BabyShop.Infrastructure.Repositories.Deliveries
{
    public class DeliveryRepository : IDeliveryRepository
    {
        private readonly AppDbContext _context;

        public DeliveryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DeliveryEntity delivery)
        {
            await _context.Deliveries.AddAsync(delivery);
            await _context.SaveChangesAsync();
        }

        public async Task<DeliveryEntity?> GetByIdAsync(int id)
        {
            return await _context.Deliveries.FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}