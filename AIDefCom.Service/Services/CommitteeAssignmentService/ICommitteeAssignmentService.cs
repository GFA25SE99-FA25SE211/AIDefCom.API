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
        Task<IEnumerable<CommitteeAssignmentReadDto>> GetAllAsync();
        Task<CommitteeAssignmentReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<CommitteeAssignmentReadDto>> GetByCouncilIdAsync(int councilId);
        Task<IEnumerable<CommitteeAssignmentReadDto>> GetBySessionIdAsync(int sessionId);
        Task<IEnumerable<CommitteeAssignmentReadDto>> GetByLecturerIdAsync(string lecturerId);
        Task<string> AddAsync(CommitteeAssignmentCreateDto dto);
        Task<bool> UpdateAsync(int id, CommitteeAssignmentUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
