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
                             .Include(x => x.User)
                             .Include(x => x.Council)
                             .Include(x => x.Session)
                             .OrderBy(x => x.Role)
                             .ToListAsync();
        }

        public async Task<CommitteeAssignment?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.User)
                             .Include(x => x.Council)
                             .Include(x => x.Session)
                             .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<CommitteeAssignment>> GetByCouncilIdAsync(int councilId)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.User)
                             .Include(x => x.Session)
                             .Where(x => x.CouncilId == councilId)
                             .ToListAsync();
        }

        public async Task<IEnumerable<CommitteeAssignment>> GetBySessionIdAsync(int sessionId)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.User)
                             .Include(x => x.Council)
                             .Where(x => x.SessionId == sessionId)
                             .ToListAsync();
        }

        public async Task<IEnumerable<CommitteeAssignment>> GetByUserIdAsync(string userId)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Council)
                             .Include(x => x.Session)
                             .Where(x => x.UserId == userId)
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

            existing.UserId = entity.UserId;
            existing.CouncilId = entity.CouncilId;
            existing.SessionId = entity.SessionId;
            existing.Role = entity.Role;
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _set.FindAsync(id);
            if (existing != null)
                _set.Remove(existing);
        }
    }
}
