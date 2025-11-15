using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.LecturerRepository
{
    public class LecturerRepository : ILecturerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Lecturer> _set;

        public LecturerRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<Lecturer>();
        }

        public async Task<IEnumerable<Lecturer>> GetAllAsync()
        {
            return await _set.AsNoTracking()
                             .OrderBy(l => l.FullName)
                             .ToListAsync();
        }

        public async Task<Lecturer?> GetByIdAsync(string id)
        {
            return await _set.AsNoTracking()
                             .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Lecturer>> GetByDepartmentAsync(string department)
        {
            return await _set.AsNoTracking()
                             .Where(l => l.Department == department)
                             .OrderBy(l => l.FullName)
                             .ToListAsync();
        }

        public async Task<IEnumerable<Lecturer>> GetByAcademicRankAsync(string academicRank)
        {
            return await _set.AsNoTracking()
                             .Where(l => l.AcademicRank == academicRank)
                             .OrderBy(l => l.FullName)
                             .ToListAsync();
        }

        public async Task AddAsync(Lecturer lecturer)
        {
            await _set.AddAsync(lecturer);
        }

        public async Task UpdateAsync(Lecturer lecturer)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == lecturer.Id);
            if (existing == null) return;

            existing.FullName = lecturer.FullName;
            existing.Email = lecturer.Email;
            existing.PhoneNumber = lecturer.PhoneNumber;
            existing.DateOfBirth = lecturer.DateOfBirth;
            existing.Gender = lecturer.Gender;
            existing.Department = lecturer.Department;
            existing.AcademicRank = lecturer.AcademicRank;
            existing.Degree = lecturer.Degree;
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _set.FindAsync(id);
            if (entity != null)
                _set.Remove(entity);
        }
    }
}
