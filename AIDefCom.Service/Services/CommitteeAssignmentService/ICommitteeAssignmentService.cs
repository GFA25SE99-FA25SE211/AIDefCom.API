using AIDefCom.Service.Dto.CommitteeAssignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.CommitteeAssignmentService
{
    public interface ICommitteeAssignmentService
    {
        Task<IEnumerable<CommitteeAssignmentReadDto>> GetAllAsync(bool includeDeleted = false);
        Task<CommitteeAssignmentReadDto?> GetByIdAsync(string id);
        Task<IEnumerable<CommitteeAssignmentReadDto>> GetByCouncilIdAsync(int councilId);
        Task<IEnumerable<CommitteeAssignmentReadDto>> GetBySessionIdAsync(int sessionId);
        Task<IEnumerable<CommitteeAssignmentReadDto>> GetByLecturerIdAsync(string lecturerId);
        Task<string> AddAsync(CommitteeAssignmentCreateDto dto);
        Task<bool> UpdateAsync(string id, CommitteeAssignmentUpdateDto dto);
        Task<bool> DeleteAsync(string id);
        Task<bool> SoftDeleteAsync(string id);
        Task<bool> RestoreAsync(string id);
    }
}
