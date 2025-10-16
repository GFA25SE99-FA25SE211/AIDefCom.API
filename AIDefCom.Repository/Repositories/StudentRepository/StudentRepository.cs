using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.StudentRepository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Student> _set;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<Student>();
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _set.AsNoTracking()
                             .Include(s => s.User)
                             .Include(s => s.Group)
                             .OrderBy(s => s.User!.FullName)
                             .ToListAsync();
        }

        public async Task<Student?> GetByIdAsync(string id)
        {
            return await _set.AsNoTracking()
                             .Include(s => s.User)
                             .Include(s => s.Group)
                             .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Student>> GetByGroupIdAsync(string groupId)
        {
            return await _set.AsNoTracking()
                             .Include(s => s.User)
                             .Where(s => s.GroupId == groupId)
                             .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetByUserIdAsync(string userId)
        {
            return await _set.AsNoTracking()
                             .Include(s => s.Group)
                             .Where(s => s.UserId == userId)
                             .ToListAsync();
        }

        public async Task AddAsync(Student student)
        {
            await _set.AddAsync(student);
        }

        public async Task UpdateAsync(Student student)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == student.Id);
            if (existing == null) return;

            existing.UserId = student.UserId;
            existing.GroupId = student.GroupId;
            existing.DateOfBirth = student.DateOfBirth;
            existing.Gender = student.Gender;
            existing.Role = student.Role;
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _set.FindAsync(id);
            if (entity != null)
                _set.Remove(entity);
        }
    }
}
