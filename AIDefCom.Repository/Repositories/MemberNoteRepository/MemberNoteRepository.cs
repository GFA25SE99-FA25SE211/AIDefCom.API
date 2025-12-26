using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.MemberNoteRepository
{
    public class MemberNoteRepository : IMemberNoteRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<MemberNote> _set;

        public MemberNoteRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<MemberNote>();
        }

        public async Task<IEnumerable<MemberNote>> GetAllAsync()
        {
            return await _set.AsNoTracking()
                             .Include(n => n.CommitteeAssignment)
                                 .ThenInclude(ca => ca.Lecturer)
                             .Include(n => n.Session)
                             .OrderByDescending(n => n.CreatedAt)
                             .ToListAsync();
        }

        public async Task<MemberNote?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking()
                             .Include(n => n.CommitteeAssignment)
                                 .ThenInclude(ca => ca.Lecturer)
                             .Include(n => n.Session)
                             .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<MemberNote>> GetBySessionIdAsync(int sessionId)
        {
            return await _set.AsNoTracking()
                             .Include(n => n.CommitteeAssignment)
                                 .ThenInclude(ca => ca.Lecturer)
                             .Include(n => n.Session)
                             .Where(n => n.SessionId == sessionId)
                             .OrderByDescending(n => n.CreatedAt)
                             .ToListAsync();
        }

        public async Task<IEnumerable<MemberNote>> GetByUserIdAsync(string userId)
        {
            return await _set.AsNoTracking()
                             .Include(n => n.CommitteeAssignment)
                                 .ThenInclude(ca => ca.Lecturer)
                             .Include(n => n.Session)
                             .Where(n => n.CommitteeAssignmentId == userId)
                             .OrderByDescending(n => n.CreatedAt)
                             .ToListAsync();
        }

        public async Task AddAsync(MemberNote note)
        {
            await _set.AddAsync(note);
        }

        public async Task UpdateAsync(MemberNote note)
        {
            var existing = await _set.FirstOrDefaultAsync(x => x.Id == note.Id);
            if (existing == null) return;

            existing.NoteContent = note.NoteContent;
            existing.SessionId = note.SessionId;
            existing.CommitteeAssignmentId = note.CommitteeAssignmentId;
        }

        public async Task DeleteAsync(int id)
            {
                var note = await _set.FindAsync(id);
                if (note != null)
                    _set.Remove(note);
            }

            public async Task HardDeleteBySessionIdsAsync(IEnumerable<int> sessionIds)
            {
                var entities = await _set.Where(x => sessionIds.Contains(x.SessionId)).ToListAsync();
                if (entities.Any())
                    _set.RemoveRange(entities);
            }
        }
}
