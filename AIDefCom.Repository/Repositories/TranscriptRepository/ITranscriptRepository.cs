using AIDefCom.Repository.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.TranscriptRepository
{
    public interface ITranscriptRepository
    {
        Task<IEnumerable<Transcript>> GetAllAsync();
        Task<Transcript?> GetByIdAsync(int id);
        Task<IEnumerable<Transcript>> GetBySessionIdAsync(int sessionId);
        Task AddAsync(Transcript entity);
        Task UpdateAsync(Transcript entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsByIdAsync(int id);
    }
}
