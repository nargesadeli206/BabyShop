using BabyShop.Core;
using BabyShop.Core.Interfaces;
using BabyShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BabyShop.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbSet.Where(e => !e.IsDeleted).ToListAsync();
    }

    public async Task<IReadOnlyList<T>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _dbSet
            .Where(e => !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        entity.IsDeleted = true;
        await UpdateAsync(entity);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync(e => !e.IsDeleted);
    }
}