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
                             .OrderBy(s => s.FullName)
                             .ToListAsync();
        }

        public async Task<Student?> GetByIdAsync(string id)
        {
            return await _set.AsNoTracking()
                             .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Student>> GetByGroupIdAsync(string groupId)
        {
            // Student không còn GroupId trực tiếp, cần query qua StudentGroup
            var studentIds = await _context.StudentGroups
                .Where(sg => sg.GroupId == groupId)
                .Select(sg => sg.UserId)
                .ToListAsync();

            return await _set.AsNoTracking()
                             .Where(s => studentIds.Contains(s.Id))
                             .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetByUserIdAsync(string userId)
        {
            return await _set.AsNoTracking()
                             .Where(s => s.Id == userId)
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

            existing.DateOfBirth = student.DateOfBirth;
            existing.Gender = student.Gender;
            // Note: Student inherits from AppUser, so FullName, Email, etc. can also be updated
            existing.FullName = student.FullName;
            existing.Email = student.Email;
            existing.PhoneNumber = student.PhoneNumber;
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _set.FindAsync(id);
            if (entity != null)
                _set.Remove(entity);
        }
    }
}
