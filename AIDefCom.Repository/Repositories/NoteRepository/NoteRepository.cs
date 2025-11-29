using AIDefCom.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.NoteRepository
{
    public class NoteRepository : INoteRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Note> _set;

        public NoteRepository(ApplicationDbContext context)
        {
            _context = context;
            _set = _context.Set<Note>();
        }

        public async Task<IEnumerable<Note>> GetAllAsync()
        {
            return await _set.AsNoTracking().Include(n => n.Session).ToListAsync();
        }

        public async Task<Note?> GetByIdAsync(int id)
        {
            return await _set.AsNoTracking().Include(n => n.Session).FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<Note?> GetBySessionIdAsync(int sessionId)
        {
            return await _set.AsNoTracking().Include(n => n.Session).FirstOrDefaultAsync(n => n.SessionId == sessionId);
        }

        public async Task AddAsync(Note note)
        {
            await _set.AddAsync(note);
        }

        public async Task UpdateAsync(Note note)
        {
            var existing = await _set.FirstOrDefaultAsync(n => n.Id == note.Id);
            if (existing == null) return;
            existing.Title = note.Title;
            existing.Content = note.Content;
            existing.UpdatedAt = System.DateTime.UtcNow;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _set.FindAsync(id);
            if (entity != null) _set.Remove(entity);
        }
    }
}