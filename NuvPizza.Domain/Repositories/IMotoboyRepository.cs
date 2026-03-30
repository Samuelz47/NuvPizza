using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuvPizza.Domain.Entities;

namespace NuvPizza.Domain.Repositories
{
    public interface IMotoboyRepository
    {
        Task<Motoboy?> GetByIdAsync(Guid id);
        Task<IEnumerable<Motoboy>> GetAllAsync();
        Task<IEnumerable<Motoboy>> GetAtivosAsync();
        Task AddAsync(Motoboy motoboy);
        Task UpdateAsync(Motoboy motoboy);
        Task DeleteAsync(Guid id);
    }
}
