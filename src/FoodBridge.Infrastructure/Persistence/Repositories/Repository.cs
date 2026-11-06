using FoodBridge.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodBridge.Infrastructure.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbSet<T> _set;

    public Repository(AppDbContext context)
    {
        _set = context.Set<T>();
    }

    public Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _set.FindAsync([id], cancellationToken).AsTask();
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        return _set.AddAsync(entity, cancellationToken).AsTask();
    }

    public void Remove(T entity)
    {
        _set.Remove(entity);
    }
}
