using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using CoreSysHM.Domain.Entities.Common;
using CoreSysHM.Domain.Interfaces.Repositories;
using CoreSysHM.Infrastructure.Data;

namespace CoreSysHM.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await _dbSet.Where(x => x.Activo).ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public async Task AddAsync(T entity) =>
        await _dbSet.AddAsync(entity);

    public void Update(T entity)
    {
        entity.FechaModificacion = DateTime.UtcNow;
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        entity.Activo = false;
        entity.FechaModificacion = DateTime.UtcNow;
        _dbSet.Update(entity);
    }
}
