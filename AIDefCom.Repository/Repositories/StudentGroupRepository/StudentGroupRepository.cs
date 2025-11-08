using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.StudentGroupRepository
{
    public class StudentGroupRepository : IStudentGroupRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<StudentGroup> _set;

        public StudentGroupRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<StudentGroup>();
        }

        public async Task<IEnumerable<StudentGroup>> GetAllAsync()
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Student)
                             .Include(x => x.Group)
                             .OrderBy(x => x.GroupId)
                             .ToListAsync();
        }

        public async Task<StudentGroup?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Student)
                             .Include(x => x.Group)
                             .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<StudentGroup>> GetByGroupIdAsync(string groupId)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Student)
                             .Where(x => x.GroupId == groupId)
                             .ToListAsync();
        }

        public async Task<IEnumerable<StudentGroup>> GetByStudentIdAsync(string studentId)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Group)
                             .Where(x => x.UserId == studentId)
                             .ToListAsync();
        }

        public async Task AddAsync(StudentGroup entity)
        {
            await _set.AddAsync(entity);
        }

        public async Task UpdateAsync(StudentGroup entity)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (existing == null) return;

            existing.UserId = entity.UserId;
            existing.GroupId = entity.GroupId;
            existing.GroupRole = entity.GroupRole;
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _set.FindAsync(id);
            if (existing != null)
                _set.Remove(existing);
        }

        public IQueryable<StudentGroup> Query()
        {
            return _set.AsQueryable();
        }
    }
}
