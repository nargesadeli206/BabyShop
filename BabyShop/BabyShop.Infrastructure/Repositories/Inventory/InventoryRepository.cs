using BabyShop.BabyShop.Core.Entities;
using BabyShop.BabyShop.Core.Interfaces;
using BabyShop.BabyShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BabyShop.BabyShop.Infrastructure.Repositories.Inventory
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(InventoryItem item)
        {
            await _context.InventoryItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(InventoryItem item)
        {
            _context.InventoryItems.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task<InventoryItem?> GetByProductIdAsync(int productId)
        {
            return await _context.InventoryItems
                .FirstOrDefaultAsync(x => x.ProductId == productId);
        }
    }
}
