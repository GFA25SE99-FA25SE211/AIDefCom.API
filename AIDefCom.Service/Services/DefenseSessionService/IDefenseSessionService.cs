using AIDefCom.Service.Dto.DefenseSession;
using AIDefCom.Service.Dto.Import;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.DefenseSessionService
{
    public interface IDefenseSessionService
    {
        Task<IEnumerable<DefenseSessionReadDto>> GetAllAsync(bool includeDeleted = false);
        Task<DefenseSessionReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<DefenseSessionReadDto>> GetByGroupIdAsync(string groupId);
        Task<IEnumerable<DefenseSessionReadDto>> GetByLecturerIdAsync(string lecturerId);
        Task<IEnumerable<DefenseSessionReadDto>> GetByStudentIdAsync(string studentId);
        Task<string?> GetLecturerRoleInDefenseSessionAsync(string lecturerId, int defenseSessionId);
        Task<int> AddAsync(DefenseSessionCreateDto dto);
        Task<bool> UpdateAsync(int id, DefenseSessionUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
        Task<IEnumerable<UserReadDto>> GetUsersByDefenseSessionIdAsync(int defenseSessionId);
        
        // Status management
        Task<bool> ChangeStatusAsync(int id, string newStatus);
        
        // Import methods
        Task<DefenseSessionImportResultDto> ImportDefenseSessionsAsync(IFormFile file);
        byte[] GenerateDefenseSessionTemplate();
    }
}
