using InventorySales.Domain.Entities.Common;
using InventorySales.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySales.Infrastructure.Repositories.Base
{
    public class BaseRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public async Task AddAsync(TEntity entity)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        // soft delete
        public async Task DeleteAsync(TEntity entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        // hard delete
        public async Task HardDeleteAsync(int id)
        {
            await _dbSet.IgnoreQueryFilters().Where(e => e.Id == id).ExecuteDeleteAsync();
        }

        
        public IQueryable<TEntity> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }
    }
}