using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<IEnumerable<MajorRubric>> GetAllAsync()
            => await _set.AsNoTracking().Include(x => x.Major).Include(x => x.Rubric).ToListAsync();

        public async Task<MajorRubric?> GetByIdAsync(int id)
            => await _set.AsNoTracking().Include(x => x.Major).Include(x => x.Rubric).FirstOrDefaultAsync(x => x.Id == id);

        public async Task<IEnumerable<MajorRubric>> GetByMajorIdAsync(int majorId)
            => await _set.AsNoTracking().Include(x => x.Rubric).Where(x => x.MajorId == majorId).ToListAsync();

        public async Task<IEnumerable<MajorRubric>> GetByRubricIdAsync(int rubricId)
            => await _set.AsNoTracking().Include(x => x.Major).Where(x => x.RubricId == rubricId).ToListAsync();

        public async Task AddAsync(MajorRubric entity) => await _set.AddAsync(entity);

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
            if (item != null) _set.Remove(item);
        }

        public async Task<bool> ExistsAsync(int majorId, int rubricId)
            => await _set.AnyAsync(x => x.MajorId == majorId && x.RubricId == rubricId);
    }
}
