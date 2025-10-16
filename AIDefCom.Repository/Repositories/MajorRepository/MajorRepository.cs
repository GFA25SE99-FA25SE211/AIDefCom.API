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

        public async Task<IEnumerable<Major>> GetAllAsync()
        {
            return await _set.AsNoTracking()
                             .OrderBy(x => x.MajorName)
                             .ToListAsync();
        }

        public async Task<Major?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
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

        public async Task<bool> ExistsByNameAsync(string majorName)
        {
            return await _set.AnyAsync(x => x.MajorName == majorName);
        }
    }
}
