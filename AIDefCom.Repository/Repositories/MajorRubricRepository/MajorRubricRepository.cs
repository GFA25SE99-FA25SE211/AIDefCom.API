using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.MajorRubricRepository
{
    public class MajorRubricRepository : IMajorRubricRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<MajorRubric> _set;

        public MajorRubricRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<MajorRubric>();
        }

        public async Task<IEnumerable<MajorRubric>> GetAllAsync(bool includeDeleted = false)
        {
            IQueryable<MajorRubric> query = _set.AsNoTracking()
                         .Include(x => x.Major)
                         .Include(x => x.Rubric);
            
            if (!includeDeleted)
                query = query.Where(x => !x.IsDeleted);
            
            return await query.ToListAsync();
        }

        public async Task<MajorRubric?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            IQueryable<MajorRubric> query = _set.AsNoTracking()
                         .Include(x => x.Major)
                         .Include(x => x.Rubric);
            
            if (!includeDeleted)
                query = query.Where(x => !x.IsDeleted);
            
            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<MajorRubric>> GetByMajorIdAsync(int majorId)
            => await _set.AsNoTracking()
                         .Include(x => x.Rubric)
                         .Where(x => x.MajorId == majorId && !x.IsDeleted)
                         .ToListAsync();

        public async Task<IEnumerable<MajorRubric>> GetByRubricIdAsync(int rubricId)
            => await _set.AsNoTracking()
                         .Include(x => x.Major)
                         .Where(x => x.RubricId == rubricId && !x.IsDeleted)
                         .ToListAsync();

        public async Task<IEnumerable<Rubric>> GetRubricsByMajorIdAsync(int majorId)
            => await _set.AsNoTracking()
                         .Include(x => x.Rubric)
                         .Where(x => x.MajorId == majorId && !x.IsDeleted && x.Rubric != null)
                         .Select(x => x.Rubric!)
                         .ToListAsync();

        public async Task<bool> ExistsAsync(int majorId, int rubricId)
            => await _set.Where(x => !x.IsDeleted).AnyAsync(x => x.MajorId == majorId && x.RubricId == rubricId);

        public async Task<bool> ExistsByMajorAndRubricNameAsync(int majorId, string rubricName)
            => await _set.AsNoTracking()
                         .Include(x => x.Rubric)
                         .AnyAsync(x => !x.IsDeleted && x.MajorId == majorId && x.Rubric != null && x.Rubric.RubricName == rubricName);

        public async Task AddAsync(MajorRubric entity)
            => await _set.AddAsync(entity);

        public async Task UpdateAsync(MajorRubric entity)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (existing == null) return;

            existing.MajorId = entity.MajorId;
            existing.RubricId = entity.RubricId;
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _set.FindAsync(id);
            if (item != null)
                _set.Remove(item);
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
    }
}
