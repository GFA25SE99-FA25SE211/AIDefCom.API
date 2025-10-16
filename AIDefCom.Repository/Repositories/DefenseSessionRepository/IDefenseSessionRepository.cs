using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.DefenseSessionRepository
{
    public interface IDefenseSessionRepository
    {
        Task<IEnumerable<DefenseSession>> GetAllAsync();
        Task<DefenseSession?> GetByIdAsync(int id);
        Task<IEnumerable<DefenseSession>> GetByGroupIdAsync(string groupId);
        Task AddAsync(DefenseSession session);
        Task UpdateAsync(DefenseSession session);
        Task DeleteAsync(int id);
    }
}
