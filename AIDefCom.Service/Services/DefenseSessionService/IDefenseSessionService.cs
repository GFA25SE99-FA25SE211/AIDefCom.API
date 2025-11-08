using AIDefCom.Service.Dto.DefenseSession;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.DefenseSessionService
{
    public interface IDefenseSessionService
    {
        Task<IEnumerable<DefenseSessionReadDto>> GetAllAsync();
        Task<DefenseSessionReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<DefenseSessionReadDto>> GetByGroupIdAsync(string groupId);
        Task<int> AddAsync(DefenseSessionCreateDto dto);
        Task<bool> UpdateAsync(int id, DefenseSessionUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<UserReadDto>> GetUsersByDefenseSessionIdAsync(int defenseSessionId);

    }
}
