using AIDefCom.Service.Dto.CouncilRole;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.CouncilRoleService
{
    public interface ICouncilRoleService
    {
        Task<IEnumerable<CouncilRoleReadDto>> GetAllAsync(bool includeDeleted = false);
        Task<CouncilRoleReadDto?> GetByIdAsync(int id);
        Task<CouncilRoleReadDto?> GetByRoleNameAsync(string roleName);
        Task<int> AddAsync(CouncilRoleCreateDto dto);
        Task<bool> UpdateAsync(int id, CouncilRoleUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
    }
}
