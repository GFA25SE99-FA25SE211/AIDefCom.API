using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.GroupRepository
{
    public class GroupRepository : IGroupRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Group> _set;

        public GroupRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<Group>();
        }

        public async Task<IEnumerable<Group>> GetAllAsync()
        {
            return await _set.AsNoTracking()
                             .Include(g => g.Semester)
                             .OrderBy(g => g.ProjectCode)
                             .ToListAsync();
        }

        public async Task<Group?> GetByIdAsync(string id)
        {
            return await _set.AsNoTracking()
                             .Include(g => g.Semester)
                             .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<Group>> GetBySemesterIdAsync(int semesterId)
        {
            return await _set.AsNoTracking()
                             .Include(g => g.Semester)
                             .Where(g => g.SemesterId == semesterId)
                             .ToListAsync();
        }

        public async Task AddAsync(Group group)
        {
            await _set.AddAsync(group);
        }

        public async Task UpdateAsync(Group group)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == group.Id);
            if (existing == null) return;

            existing.ProjectCode = group.ProjectCode;
            existing.TopicTitle_EN = group.TopicTitle_EN;
            existing.TopicTitle_VN = group.TopicTitle_VN;
            existing.SemesterId = group.SemesterId;
            existing.Status = group.Status;
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _set.FindAsync(id);
            if (entity != null)
                _set.Remove(entity);
        }

        public async Task<bool> ExistsByProjectCodeAsync(string code)
        {
            return await _set.AnyAsync(g => g.ProjectCode == code);
        }
    }
}
