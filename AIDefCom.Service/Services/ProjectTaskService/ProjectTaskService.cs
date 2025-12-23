using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.ProjectTask;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.ProjectTaskService
{
    public class ProjectTaskService : IProjectTaskService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ProjectTaskService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProjectTaskReadDto>> GetAllAsync()
        {
            var list = await _uow.ProjectTasks.GetAllAsync();
            return list.Select(t => new ProjectTaskReadDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                AssignedById = t.AssignedById,
                AssignedByName = t.AssignedBy?.Lecturer?.FullName,
                AssignedToId = t.AssignedToId,
                AssignedToName = t.AssignedTo?.Lecturer?.FullName,
                RubricId = t.RubricId,
                Status = t.Status
            });
        }

        public async Task<ProjectTaskReadDto?> GetByIdAsync(int id)
        {
            var entity = await _uow.ProjectTasks.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<ProjectTaskReadDto>(entity);
        }

        public async Task<IEnumerable<ProjectTaskReadDto>> GetByAssignerAsync(string assignedById)
        {
            var list = await _uow.ProjectTasks.GetByAssignerAsync(assignedById);
            return _mapper.Map<IEnumerable<ProjectTaskReadDto>>(list);
        }

        public async Task<IEnumerable<ProjectTaskReadDto>> GetByAssigneeAsync(string assignedToId)
        {
            var list = await _uow.ProjectTasks.GetByAssigneeAsync(assignedToId);
            return _mapper.Map<IEnumerable<ProjectTaskReadDto>>(list);
        }

        public async Task<IEnumerable<ProjectTaskReadDto>> GetByAssigneeAndSessionAsync(string assignedToId, int sessionId)
        {
            var list = await _uow.ProjectTasks.GetByAssigneeAndSessionAsync(assignedToId, sessionId);
            return _mapper.Map<IEnumerable<ProjectTaskReadDto>>(list);
        }

        public async Task<IEnumerable<string>> GetRubricNamesByAssigneeAndSessionAsync(string assignedToId, int sessionId)
        {
            var list = await _uow.ProjectTasks.GetByAssigneeAndSessionAsync(assignedToId, sessionId);
            return list.Select(t => t.Rubric?.RubricName).Where(n => !string.IsNullOrEmpty(n))!.Distinct()!;
        }

        public async Task<IEnumerable<string>> GetRubricNamesByLecturerAndSessionAsync(string lecturerId, int sessionId)
        {
            // Convert LecturerId to CommitteeAssignmentId
            var committeeAssignmentId = await _uow.CommitteeAssignments.GetByLecturerIdAndSessionIdAsync(lecturerId, sessionId);
            
            if (committeeAssignmentId == null)
            {
                throw new KeyNotFoundException($"No committee assignment found for lecturer '{lecturerId}' in session {sessionId}");
            }

            // Use the CommitteeAssignmentId to get rubric names
            var list = await _uow.ProjectTasks.GetByAssigneeAndSessionAsync(committeeAssignmentId.Id, sessionId);
            return list.Select(t => t.Rubric?.RubricName).Where(n => !string.IsNullOrEmpty(n))!.Distinct()!;
        }

        public async Task<int?> GetRubricIdByNameAsync(string rubricName)
        {
            if (string.IsNullOrWhiteSpace(rubricName))
            {
                throw new ArgumentException("rubricName is required", nameof(rubricName));
            }

            var rubric = await _uow.Rubrics.GetByNameAsync(rubricName);
            return rubric?.Id;
        }

        public async Task<int> AddAsync(ProjectTaskCreateDto dto)
        {
            if (dto.SessionId <= 0)
            {
                throw new ArgumentException("SessionId is required and must be a positive integer");
            }

            // Ensure DefenseSession exists
            var session = await _uow.DefenseSessions.GetByIdAsync(dto.SessionId);
            if (session == null)
            {
                throw new ArgumentException($"DefenseSession with ID {dto.SessionId} not found");
            }

            // Validate uniqueness: one rubric per session
            var rubricClash = await _uow.ProjectTasks.ExistsBySessionAndRubricAsync(dto.SessionId, dto.RubricId);
            if (rubricClash)
            {
                throw new InvalidOperationException($"A ProjectTask with RubricId {dto.RubricId} already exists in DefenseSession {dto.SessionId}");
            }

            // ✅ Validate AssignedById - If dto contains LecturerId, find CommitteeAssignment
            var assignedBy = await _uow.CommitteeAssignments.GetByLecturerIdAndSessionIdAsync(dto.AssignedById, dto.SessionId);
            if (assignedBy == null)
            {
                throw new ArgumentException($"No active CommitteeAssignment found for Lecturer ID '{dto.AssignedById}' (AssignedById)");
            }

            // ✅ Validate AssignedToId - If dto contains LecturerId, find CommitteeAssignment
            var assignedTo = await _uow.CommitteeAssignments.GetByLecturerIdAndSessionIdAsync(dto.AssignedToId, dto.SessionId);
            if (assignedTo == null)
            {
                throw new ArgumentException($"No active CommitteeAssignment found for Lecturer ID '{dto.AssignedToId}' (AssignedToId)");
            }

            // ✅ Validate RubricId exists
            var rubric = await _uow.Rubrics.GetByIdAsync(dto.RubricId);
            if (rubric == null)
            {
                throw new ArgumentException($"Rubric with ID {dto.RubricId} not found");
            }

            // Map DTO to Entity
            var entity = _mapper.Map<ProjectTask>(dto);
            
            // ⚠️ IMPORTANT: Override with CommitteeAssignmentId (not LecturerId)
            entity.AssignedById = assignedBy.Id;
            entity.AssignedToId = assignedTo.Id;
            entity.SessionId = dto.SessionId;
            
            try
            {
                await _uow.ProjectTasks.AddAsync(entity);
                await _uow.SaveChangesAsync();
                return entity.Id;
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException($"Database error while saving ProjectTask: {innerMessage}", ex);
            }
        }

        public async Task<bool> UpdateAsync(int id, ProjectTaskUpdateDto dto)
        {
            var existing = await _uow.ProjectTasks.GetByIdAsync(id);
            if (existing == null) return false;

            if (dto.SessionId <= 0)
            {
                throw new ArgumentException("SessionId is required and must be a positive integer");
            }
            var session = await _uow.DefenseSessions.GetByIdAsync(dto.SessionId);
            if (session == null)
            {
                throw new ArgumentException($"DefenseSession with ID {dto.SessionId} not found");
            }

            // Validate uniqueness on update (exclude current task)
            var rubricClash = await _uow.ProjectTasks.ExistsBySessionAndRubricAsync(dto.SessionId, dto.RubricId, id);
            if (rubricClash)
            {
                throw new InvalidOperationException($"A ProjectTask with RubricId {dto.RubricId} already exists in DefenseSession {dto.SessionId}");
            }

            _mapper.Map(dto, existing);
            await _uow.ProjectTasks.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _uow.ProjectTasks.GetByIdAsync(id);
            if (entity == null) return false;

            await _uow.ProjectTasks.DeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
