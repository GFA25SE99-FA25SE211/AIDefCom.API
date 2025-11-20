using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.MajorRepository
{
    public class MajorRepository : IMajorRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Major> _set;

        public MajorRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<Major>();
        }

        public async Task<IEnumerable<Major>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _set.AsNoTracking();
            
            if (!includeDeleted)
                query = query.Where(x => !x.IsDeleted);
            
            return await query.OrderBy(x => x.MajorName).ToListAsync();
        }

        public async Task<Major?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            var query = _set.AsNoTracking();
            
            if (!includeDeleted)
                query = query.Where(x => !x.IsDeleted);
            
            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(Major major)
        {
            await _set.AddAsync(major);
        }

        public async Task UpdateAsync(Major major)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == major.Id);
            if (existing == null) return;

            existing.MajorName = major.MajorName;
            existing.Description = major.Description;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _set.FindAsync(id);
            if (entity != null) _set.Remove(entity);
        }

        public async Task SoftDeleteAsync(int id)
        {
            var entity = await _set.FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                entity.IsDeleted = true;
            }
        }

        public async Task RestoreAsync(int id)
        {
            var entity = await _set.FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                entity.IsDeleted = false;
            }
        }

        public async Task<bool> ExistsByNameAsync(string majorName)
        {
            return await _set.Where(x => !x.IsDeleted).AnyAsync(x => x.MajorName == majorName);
        }
    }
}
