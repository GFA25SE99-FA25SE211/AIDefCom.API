using AIDefCom.Service.Dto.MajorRubric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.MajorRubricService
{
    public interface IMajorRubricService
    {
        Task<IEnumerable<MajorRubricReadDto>> GetAllAsync(bool includeDeleted = false);
        Task<IEnumerable<MajorRubricReadDto>> GetByMajorIdAsync(int majorId);
        Task<IEnumerable<MajorRubricReadDto>> GetByRubricIdAsync(int rubricId);
        Task<int> AddAsync(MajorRubricCreateDto dto);
        Task<bool> UpdateAsync(int id, MajorRubricUpdateDto dto);  
        Task<bool> DeleteAsync(int id);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
    }
}
