using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.CommitteeAssignmentRepository
{
    public class CommitteeAssignmentRepository : ICommitteeAssignmentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<CommitteeAssignment> _set;

        public CommitteeAssignmentRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<CommitteeAssignment>();
        }

        public async Task<IEnumerable<CommitteeAssignment>> GetAllAsync()
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Lecturer)
                             .Include(x => x.Council)
                             .Include(x => x.CouncilRole)
                             .OrderBy(x => x.CouncilRole!.RoleName)
                             .ToListAsync();
        }

        public async Task<CommitteeAssignment?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Lecturer)
                             .Include(x => x.Council)
                             .Include(x => x.CouncilRole)
                             .FirstOrDefaultAsync(x => x.Id == id.ToString());
        }

        public async Task<IEnumerable<CommitteeAssignment>> GetByCouncilIdAsync(int councilId)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Lecturer)
                             .Include(x => x.CouncilRole)
                             .Where(x => x.CouncilId == councilId)
                             .ToListAsync();
        }

        public async Task<IEnumerable<CommitteeAssignment>> GetBySessionIdAsync(int sessionId)
        {
            // CommitteeAssignment không còn SessionId
            // Có thể query qua DefenseSession -> Council -> CommitteeAssignment
            var councilIds = await _context.DefenseSessions
                .Where(ds => ds.Id == sessionId)
                .Select(ds => ds.CouncilId)
                .ToListAsync();

            return await _set.AsNoTracking()
                             .Include(x => x.Lecturer)
                             .Include(x => x.Council)
                             .Include(x => x.CouncilRole)
                             .Where(x => councilIds.Contains(x.CouncilId))
                             .ToListAsync();
        }

        public async Task<IEnumerable<CommitteeAssignment>> GetByLecturerIdAsync(string lecturerId)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Council)
                             .Include(x => x.CouncilRole)
                             .Where(x => x.LecturerId == lecturerId)
                             .ToListAsync();
        }

        public async Task AddAsync(CommitteeAssignment entity)
        {
            await _set.AddAsync(entity);
        }

        public async Task UpdateAsync(CommitteeAssignment entity)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (existing == null) return;

            existing.LecturerId = entity.LecturerId;
            existing.CouncilId = entity.CouncilId;
            existing.CouncilRoleId = entity.CouncilRoleId;
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _set.FindAsync(id.ToString());
            if (existing != null)
                _set.Remove(existing);
        }
        public IQueryable<CommitteeAssignment> Query()
        {
            return _set.AsQueryable();
        }

    }
}
