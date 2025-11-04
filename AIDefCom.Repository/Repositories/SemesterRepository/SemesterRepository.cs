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

        public async Task<IEnumerable<Semester>> GetAllAsync()
        {
            return await _set.AsNoTracking()
                             .OrderByDescending(s => s.Year)
                             .ThenBy(s => s.SemesterName)
                             .ToListAsync();
        }

        public async Task<Semester?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking()
                             .FirstOrDefaultAsync(s => s.Id == id);
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

        public async Task<bool> ExistsByNameAsync(string name, int year, int majorId)
        {
            // Semester không còn MajorId, chỉ check name và year
            return await _set.AnyAsync(s =>
                s.SemesterName == name &&
                s.Year == year);
        }
    }
}
