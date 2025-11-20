using AIDefCom.Service.Dto.Semester;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.SemesterService
{
    public interface ISemesterService
    {
        Task<IEnumerable<SemesterReadDto>> GetAllAsync(bool includeDeleted = false);
        Task<SemesterReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<SemesterReadDto>> GetByMajorIdAsync(int majorId);
        Task<int> AddAsync(SemesterCreateDto dto);
        Task<bool> UpdateAsync(int id, SemesterUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
    }
}
