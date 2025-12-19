using AIDefCom.Service.Dto.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.GroupService
{
    public interface IGroupService
    {
        Task<IEnumerable<GroupReadDto>> GetAllAsync(bool includeDeleted = false);
        Task<GroupReadDto?> GetByIdAsync(string id);
        Task<IEnumerable<GroupReadDto>> GetBySemesterIdAsync(int semesterId);
        Task<string> AddAsync(GroupCreateDto dto);
        Task<bool> UpdateAsync(string id, GroupUpdateDto dto);
        Task<bool> DeleteAsync(string id);
        Task<bool> RestoreAsync(string id);
    }
}
