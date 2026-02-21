using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;

namespace NuvPizza.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        IQueryable<T> query = _context.Set<T>().AsNoTracking();
        // Se T for Produto, carrega as templates
        if (typeof(T) == typeof(Produto))
        {
            query = query.Include("ComboTemplates");
        }
        return await query.ToListAsync();
    }
    public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
    {
        IQueryable<T> query = _context.Set<T>();
        if (typeof(T) == typeof(Produto))
        {
            query = query.Include("ComboTemplates");
        }
        return await query.FirstOrDefaultAsync(predicate);
    }

    public T Create(T entity)
    {
        _context.Set<T>().Add(entity);

        return entity;
    }

    public T Update(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;

        return entity;
    }

    public T Delete(T entity)
    {
        _context.Set<T>().Remove(entity);

        return entity;
    }
}