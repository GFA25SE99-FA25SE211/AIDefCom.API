using AIDefCom.Service.Dto.Rubric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.RubricService
{
    public interface IRubricService
    {
        Task<IEnumerable<RubricReadDto>> GetAllAsync();
        Task<RubricReadDto?> GetByIdAsync(int id);
        Task<int> AddAsync(RubricCreateDto dto);
        Task<bool> UpdateAsync(int id, RubricUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
