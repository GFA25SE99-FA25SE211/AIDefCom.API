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
                             .Include(n => n.Group)
                             .OrderByDescending(n => n.CreatedAt)
                             .ToListAsync();
        }

        public async Task<MemberNote?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking()
                             .Include(n => n.CommitteeAssignment)
                                 .ThenInclude(ca => ca.Lecturer)
                             .Include(n => n.Group)
                             .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<MemberNote>> GetByGroupIdAsync(string groupId)
        {
            return await _set.AsNoTracking()
                             .Include(n => n.CommitteeAssignment)
                                 .ThenInclude(ca => ca.Lecturer)
                             .Where(n => n.GroupId == groupId)
                             .OrderByDescending(n => n.CreatedAt)
                             .ToListAsync();
        }

        public async Task<IEnumerable<MemberNote>> GetByUserIdAsync(string userId)
        {
            return await _set.AsNoTracking()
                             .Include(n => n.CommitteeAssignment)
                                 .ThenInclude(ca => ca.Lecturer)
                             .Include(n => n.Group)
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
            existing.GroupId = note.GroupId;
            existing.CommitteeAssignmentId = note.CommitteeAssignmentId;
        }

        public async Task DeleteAsync(int id)
        {
            var note = await _set.FindAsync(id);
            if (note != null)
                _set.Remove(note);
        }
    }
}
