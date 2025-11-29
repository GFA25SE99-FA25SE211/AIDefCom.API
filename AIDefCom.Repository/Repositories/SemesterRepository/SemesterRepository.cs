using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.SemesterRepository
{
    public class SemesterRepository : ISemesterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Semester> _set;

        public SemesterRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<Semester>();
        }

        public async Task<IEnumerable<Semester>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _set.AsNoTracking();
            
            if (!includeDeleted)
                query = query.Where(x => !x.IsDeleted);
            
            return await query.OrderByDescending(s => s.Year)
                             .ThenBy(s => s.SemesterName)
                             .ToListAsync();
        }

        public async Task<Semester?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            var query = _set.AsNoTracking();
            
            if (!includeDeleted)
                query = query.Where(x => !x.IsDeleted);
            
            return await query.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Semester>> GetByMajorIdAsync(int majorId)
        {
            // Semester không còn MajorId, method này có thể bỏ hoặc return empty
            return await Task.FromResult(Enumerable.Empty<Semester>());
        }

        public async Task AddAsync(Semester semester)
        {
            await _set.AddAsync(semester);
        }

        public async Task UpdateAsync(Semester semester)
        {
            var existing = await _set.FirstOrDefaultAsync(s => s.Id == semester.Id);
            if (existing == null) return;

            existing.SemesterName = semester.SemesterName;
            existing.Year = semester.Year;
            existing.StartDate = semester.StartDate;
            existing.EndDate = semester.EndDate;
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

        public async Task<bool> ExistsByNameAsync(string name, int year, int majorId)
        {
            // Case-insensitive, trimmed comparison; ignore MajorId
            var normalized = name.Trim().ToLower();
            return await _set
                .Where(x => !x.IsDeleted)
                .AnyAsync(s => s.Year == year && s.SemesterName != null && s.SemesterName.Trim().ToLower() == normalized);
        }
    }
}
