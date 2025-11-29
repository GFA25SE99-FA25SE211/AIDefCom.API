using AIDefCom.Repository.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.NoteRepository
{
    public interface INoteRepository
    {
        Task<IEnumerable<Note>> GetAllAsync();
        Task<Note?> GetByIdAsync(int id);
        Task<Note?> GetBySessionIdAsync(int sessionId);
        Task AddAsync(Note note);
        Task UpdateAsync(Note note);
        Task DeleteAsync(int id);
    }
}