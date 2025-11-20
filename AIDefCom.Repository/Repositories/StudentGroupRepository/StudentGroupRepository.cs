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

        public async Task<IEnumerable<StudentGroup>> GetAllAsync(bool includeDeleted = false)
        {
            IQueryable<StudentGroup> query = _set.AsNoTracking()
                             .Include(x => x.Student)
                             .Include(x => x.Group);
            
            if (!includeDeleted)
                query = query.Where(x => !x.IsDeleted);
            
            return await query.OrderBy(x => x.GroupId).ToListAsync();
        }

        public async Task<StudentGroup?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            IQueryable<StudentGroup> query = _set.AsNoTracking()
                             .Include(x => x.Student)
                             .Include(x => x.Group);
            
            if (!includeDeleted)
                query = query.Where(x => !x.IsDeleted);
            
            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<StudentGroup>> GetByGroupIdAsync(string groupId)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Student)
                             .Where(x => x.GroupId == groupId && !x.IsDeleted)
                             .ToListAsync();
        }

        public async Task<IEnumerable<StudentGroup>> GetByStudentIdAsync(string studentId)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Group)
                             .Where(x => x.UserId == studentId && !x.IsDeleted)
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

        public IQueryable<StudentGroup> Query()
        {
            return _set.AsQueryable();
        }
    }
}
