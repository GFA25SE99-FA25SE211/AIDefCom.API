using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.CouncilRepository
{
    public class CouncilRepository : ICouncilRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Council> _set;

        public CouncilRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<Council>();
        }

        public async Task<IEnumerable<Council>> GetAllAsync(bool includeInactive = false)
        {
            IQueryable<Council> query = _set.AsNoTracking().Include(c => c.Major);
            if (!includeInactive)
                query = query.Where(c => c.IsActive);
            return await query.OrderByDescending(c => c.CreatedDate).ToListAsync();
        }

        public async Task<Council?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking()
                             .Include(c => c.Major)
                             .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(Council entity)
        {
            await _set.AddAsync(entity);
        }

        public async Task UpdateAsync(Council entity)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (existing == null) return;

            existing.MajorId = entity.MajorId;
            existing.Description = entity.Description;
            existing.IsActive = entity.IsActive;
        }

        public async Task SoftDeleteAsync(int id)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == id);
            if (existing != null)
                existing.IsActive = false; 
        }

        public async Task RestoreAsync(int id)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == id);
            if (existing != null)
                existing.IsActive = true; 
        }
    }
}
