using AIDefCom.Service.Dto.ProjectTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.ProjectTaskService
{
    public interface IProjectTaskService
    {
        Task<IEnumerable<ProjectTaskReadDto>> GetAllAsync();
        Task<ProjectTaskReadDto?> GetByIdAsync(int id);
        Task<IEnumerable<ProjectTaskReadDto>> GetByAssignerAsync(string assignedById);
        Task<IEnumerable<ProjectTaskReadDto>> GetByAssigneeAsync(string assignedToId);
        Task<IEnumerable<ProjectTaskReadDto>> GetByAssigneeAndSessionAsync(string assignedToId, int sessionId);
        Task<IEnumerable<string>> GetRubricNamesByAssigneeAndSessionAsync(string assignedToId, int sessionId);
        Task<IEnumerable<string>> GetRubricNamesByLecturerAndSessionAsync(string lecturerId, int sessionId);
        Task<int?> GetRubricIdByNameAsync(string rubricName);
        Task<int> AddAsync(ProjectTaskCreateDto dto);
        Task<bool> UpdateAsync(int id, ProjectTaskUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
