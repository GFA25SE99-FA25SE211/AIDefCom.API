using AIDefCom.Repository.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.CouncilRoleRepository
{
    public interface ICouncilRoleRepository
    {
        Task<IEnumerable<CouncilRole>> GetAllAsync(bool includeDeleted = false);
        Task<CouncilRole?> GetByIdAsync(int id, bool includeDeleted = false);
        Task<CouncilRole?> GetByRoleNameAsync(string roleName, bool includeDeleted = false);
        Task AddAsync(CouncilRole entity);
        Task UpdateAsync(CouncilRole entity);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
        Task RestoreAsync(int id);
    }
}
