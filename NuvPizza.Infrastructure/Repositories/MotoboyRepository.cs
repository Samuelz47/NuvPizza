using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NuvPizza.Domain.Entities;
using NuvPizza.Domain.Repositories;
using NuvPizza.Infrastructure.Persistence;

namespace NuvPizza.Infrastructure.Repositories
{
    public class MotoboyRepository : IMotoboyRepository
    {
        private readonly AppDbContext _context;

        public MotoboyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Motoboy?> GetByIdAsync(Guid id)
        {
            return await _context.Motoboys.FindAsync(id);
        }

        public async Task<IEnumerable<Motoboy>> GetAllAsync()
        {
            return await _context.Motoboys.ToListAsync();
        }

        public async Task<IEnumerable<Motoboy>> GetAtivosAsync()
        {
            return await _context.Motoboys.Where(m => m.Ativo).ToListAsync();
        }

        public async Task AddAsync(Motoboy motoboy)
        {
            await _context.Motoboys.AddAsync(motoboy);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Motoboy motoboy)
        {
            _context.Motoboys.Update(motoboy);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var motoboy = await _context.Motoboys.FindAsync(id);
            if (motoboy != null)
            {
                _context.Motoboys.Remove(motoboy);
                await _context.SaveChangesAsync();
            }
        }
    }
}
