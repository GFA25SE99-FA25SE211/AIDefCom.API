using AIDefCom.Service.Dto.Major;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.MajorService
{
    public interface IMajorService
    {
        Task<IEnumerable<MajorReadDto>> GetAllAsync(bool includeDeleted = false);
        Task<MajorReadDto?> GetByIdAsync(int id);
        Task<int> AddAsync(MajorCreateDto dto);
        Task<bool> UpdateAsync(int id, MajorUpdateDto dto);
        Task<bool> DeleteAsync(int id);  // Deprecated - use SoftDeleteAsync
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
    }
}
