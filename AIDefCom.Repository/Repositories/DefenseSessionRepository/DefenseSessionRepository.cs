using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.DefenseSessionRepository
{
    public class DefenseSessionRepository : IDefenseSessionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<DefenseSession> _set;

        public DefenseSessionRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<DefenseSession>();
        }

        public async Task<IEnumerable<DefenseSession>> GetAllAsync(bool includeDeleted = false)
        {
            IQueryable<DefenseSession> query = _set.AsNoTracking()
                             .Include(x => x.Group)
                             .Include(x => x.Council);
            
            if (!includeDeleted)
                query = query.Where(x => !x.IsDeleted);
            
            return await query.OrderByDescending(x => x.DefenseDate).ToListAsync();
        }

        public async Task<DefenseSession?> GetByIdAsync(int id, bool includeDeleted = false)
        {
            IQueryable<DefenseSession> query = _set.AsNoTracking()
                             .Include(x => x.Group)
                             .Include(x => x.Council);
            
            if (!includeDeleted)
                query = query.Where(x => !x.IsDeleted);
            
            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<DefenseSession>> GetByGroupIdAsync(string groupId)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Group)
                             .Include(x => x.Council)
                             .Where(x => x.GroupId == groupId && !x.IsDeleted)
                             .ToListAsync();
        }

        public async Task<IEnumerable<DefenseSession>> GetByLecturerIdAsync(string lecturerId)
        {
            // Get all CouncilIds where the lecturer is assigned
            var councilIds = await _context.CommitteeAssignments
                .Where(ca => ca.LecturerId == lecturerId && !ca.IsDeleted)
                .Select(ca => ca.CouncilId)
                .Distinct()
                .ToListAsync();

            // Get all defense sessions with those CouncilIds
            return await _set.AsNoTracking()
                             .Include(x => x.Group)
                             .Include(x => x.Council)
                             .Where(x => councilIds.Contains(x.CouncilId) && !x.IsDeleted)
                             .OrderByDescending(x => x.DefenseDate)
                             .ToListAsync();
        }

        public async Task<string?> GetLecturerRoleInDefenseSessionAsync(string lecturerId, int defenseSessionId)
        {
            // Get the defense session to find its CouncilId
            var defenseSession = await _set.AsNoTracking()
                .Where(x => x.Id == defenseSessionId && !x.IsDeleted)
                .FirstOrDefaultAsync();

            if (defenseSession == null)
                return null;

            // Get the committee assignment for this lecturer in this council
            var assignment = await _context.CommitteeAssignments
                .AsNoTracking()
                .Include(ca => ca.CouncilRole)
                .Where(ca => ca.LecturerId == lecturerId 
                          && ca.CouncilId == defenseSession.CouncilId 
                          && !ca.IsDeleted)
                .FirstOrDefaultAsync();

            return assignment?.CouncilRole?.RoleName;
        }

        public async Task AddAsync(DefenseSession session)
        {
            await _set.AddAsync(session);
        }

        public async Task UpdateAsync(DefenseSession session)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == session.Id);
            if (existing == null) return;

            existing.GroupId = session.GroupId;
            existing.Location = session.Location;
            existing.DefenseDate = session.DefenseDate;
            existing.StartTime = session.StartTime;
            existing.EndTime = session.EndTime;
            existing.Status = session.Status;
            existing.CouncilId = session.CouncilId;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _set.FindAsync(id);
            if (entity != null)
                _set.Remove(entity);
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

        public IQueryable<DefenseSession> Query()
        {
            return _set.AsQueryable();
        }

        public async Task<DefenseSession?> GetWithCouncilAsync(int id)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Group)
                             .Include(x => x.Council)
                             .Where(x => !x.IsDeleted)
                             .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
