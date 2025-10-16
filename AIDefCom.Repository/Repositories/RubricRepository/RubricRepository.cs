using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.RubricRepository
{
    public class RubricRepository : IRubricRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Rubric> _set;

        public RubricRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<Rubric>();
        }

        public async Task<IEnumerable<Rubric>> GetAllAsync()
        {
            return await _set.AsNoTracking()
                             .OrderByDescending(x => x.CreatedAt)
                             .ToListAsync();
        }

        public async Task<Rubric?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(Rubric rubric)
        {
            rubric.CreatedAt = DateTime.UtcNow;
            await _set.AddAsync(rubric);
        }

        public async Task UpdateAsync(Rubric rubric)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == rubric.Id);
            if (existing == null) return;

            existing.RubricName = rubric.RubricName;
            existing.Description = rubric.Description;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _set.FindAsync(id);
            if (entity != null) _set.Remove(entity);
        }

        public async Task<bool> ExistsByNameAsync(string rubricName)
        {
            return await _set.AnyAsync(x => x.RubricName == rubricName);
        }
    }
}
