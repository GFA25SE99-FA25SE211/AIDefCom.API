using AIDefCom.Service.Dto.Council;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.CouncilService
{
    public interface ICouncilService
    {
        Task<IEnumerable<CouncilReadDto>> GetAllAsync(bool includeInactive = false);
        Task<CouncilReadDto?> GetByIdAsync(int id);
        Task<int> AddAsync(CouncilCreateDto dto);
        Task<bool> UpdateAsync(int id, CouncilUpdateDto dto);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
    }
}