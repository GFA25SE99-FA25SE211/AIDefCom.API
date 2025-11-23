using AIDefCom.Repository.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.DefenseSessionRepository
{
    public interface IDefenseSessionRepository
    {
        Task<IEnumerable<DefenseSession>> GetAllAsync(bool includeDeleted = false);
        Task<DefenseSession?> GetByIdAsync(int id, bool includeDeleted = false);
        Task<IEnumerable<DefenseSession>> GetByGroupIdAsync(string groupId);
        Task<IEnumerable<DefenseSession>> GetByLecturerIdAsync(string lecturerId);
        Task<string?> GetLecturerRoleInDefenseSessionAsync(string lecturerId, int defenseSessionId);
        Task AddAsync(DefenseSession session);
        Task UpdateAsync(DefenseSession session);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
        Task RestoreAsync(int id);
        IQueryable<DefenseSession> Query();
        Task<DefenseSession?> GetWithCouncilAsync(int id);
    }
}
