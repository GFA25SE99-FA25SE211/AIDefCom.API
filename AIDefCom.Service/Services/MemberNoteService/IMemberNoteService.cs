using AIDefCom.Service.Dto.MemberNote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.MemberNoteService
{
    public interface IMemberNoteService
    {
        Task<IEnumerable<MemberNoteReadDto>> GetAllAsync();
        Task<MemberNoteReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<MemberNoteReadDto>> GetByGroupIdAsync(string groupId);
        Task<IEnumerable<MemberNoteReadDto>> GetByUserIdAsync(string userId);
        Task<int> AddAsync(MemberNoteCreateDto dto);
        Task<bool> UpdateAsync(int id, MemberNoteUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
