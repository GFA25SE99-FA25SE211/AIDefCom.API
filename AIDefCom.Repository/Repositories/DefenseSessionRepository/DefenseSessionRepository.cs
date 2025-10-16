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

        public async Task<IEnumerable<DefenseSession>> GetAllAsync()
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Group)
                             .OrderByDescending(x => x.DefenseDate)
                             .ToListAsync();
        }

        public async Task<DefenseSession?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Group)
                             .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<DefenseSession>> GetByGroupIdAsync(string groupId)
        {
            return await _set.AsNoTracking()
                             .Include(x => x.Group)
                             .Where(x => x.GroupId == groupId)
                             .ToListAsync();
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
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _set.FindAsync(id);
            if (entity != null)
                _set.Remove(entity);
        }
    }
}
