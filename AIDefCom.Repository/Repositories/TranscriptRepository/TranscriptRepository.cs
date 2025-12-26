using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.TranscriptRepository
{
    public class TranscriptRepository : ITranscriptRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Transcript> _set;

        public TranscriptRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<Transcript>();
        }

        public async Task<IEnumerable<Transcript>> GetAllAsync()
        {
            return await _set.AsNoTracking()
                             .Include(t => t.Session)
                             .OrderByDescending(t => t.CreatedAt)
                             .ToListAsync();
        }

        public async Task<Transcript?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking()
                             .Include(t => t.Session)
                             .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Transcript>> GetBySessionIdAsync(int sessionId)
        {
            return await _set.AsNoTracking()
                             .Where(t => t.SessionId == sessionId)
                             .OrderByDescending(t => t.CreatedAt)
                             .ToListAsync();
        }

        public async Task AddAsync(Transcript entity)
        {
            await _set.AddAsync(entity);
        }

        public async Task UpdateAsync(Transcript entity)
        {
            var existing = await _set.FirstOrDefaultAsync(t => t.Id == entity.Id);
            if (existing == null) return;

            existing.SessionId = entity.SessionId;
            existing.TranscriptText = entity.TranscriptText;
            existing.IsApproved = entity.IsApproved;
            existing.Status = entity.Status;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _set.FindAsync(id);
            if (entity != null) _set.Remove(entity);
        }

        public async Task HardDeleteBySessionIdsAsync(IEnumerable<int> sessionIds)
        {
            var entities = await _set.Where(x => sessionIds.Contains(x.SessionId)).ToListAsync();
            if (entities.Any())
                _set.RemoveRange(entities);
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            return await _set.AnyAsync(t => t.Id == id);
        }
    }
}
