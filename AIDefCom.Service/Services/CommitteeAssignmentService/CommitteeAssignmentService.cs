using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.CommitteeAssignment;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.CommitteeAssignmentService
{
    public class CommitteeAssignmentService : ICommitteeAssignmentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CommitteeAssignmentService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CommitteeAssignmentReadDto>> GetAllAsync(bool includeDeleted = false)
        {
            var list = await _uow.CommitteeAssignments.GetAllAsync(includeDeleted);
            return list.Select(a => new CommitteeAssignmentReadDto
            {
                Id = a.Id,
                LecturerId = a.LecturerId,
                LecturerName = a.Lecturer?.FullName,
                CouncilId = a.CouncilId,
                CouncilRoleId = a.CouncilRoleId,
                RoleName = a.CouncilRole?.RoleName
            });
        }

        public async Task<CommitteeAssignmentReadDto?> GetByIdAsync(string id)
        {
            // Validate ID
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Committee Assignment ID cannot be null or empty", nameof(id));

            var entity = await _uow.CommitteeAssignments.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<CommitteeAssignmentReadDto>(entity);
        }

        public async Task<IEnumerable<CommitteeAssignmentReadDto>> GetByCouncilIdAsync(int councilId)
        {
            // Validate CouncilId
            if (councilId <= 0)
                throw new ArgumentException("Council ID must be greater than 0", nameof(councilId));

            var list = await _uow.CommitteeAssignments.GetByCouncilIdAsync(councilId);
            return _mapper.Map<IEnumerable<CommitteeAssignmentReadDto>>(list);
        }

        public async Task<IEnumerable<CommitteeAssignmentReadDto>> GetBySessionIdAsync(int sessionId)
        {
            // Validate SessionId
            if (sessionId <= 0)
                throw new ArgumentException("Session ID must be greater than 0", nameof(sessionId));

            var list = await _uow.CommitteeAssignments.GetBySessionIdAsync(sessionId);
            return _mapper.Map<IEnumerable<CommitteeAssignmentReadDto>>(list);
        }

        public async Task<IEnumerable<CommitteeAssignmentReadDto>> GetByLecturerIdAsync(string lecturerId)
        {
            // Validate LecturerId
            if (string.IsNullOrWhiteSpace(lecturerId))
                throw new ArgumentException("Lecturer ID cannot be null or empty", nameof(lecturerId));

            var list = await _uow.CommitteeAssignments.GetByLecturerIdAsync(lecturerId);
            return _mapper.Map<IEnumerable<CommitteeAssignmentReadDto>>(list);
        }

        public async Task<string?> GetIdByLecturerIdAndSessionIdAsync(string lecturerId, int sessionId)
        {
            // Validate LecturerId
            if (string.IsNullOrWhiteSpace(lecturerId))
                throw new ArgumentException("Lecturer ID cannot be null or empty", nameof(lecturerId));

            // Validate SessionId
            if (sessionId <= 0)
                throw new ArgumentException("Session ID must be greater than 0", nameof(sessionId));

            var assignment = await _uow.CommitteeAssignments.GetByLecturerIdAndSessionIdAsync(lecturerId, sessionId);
            return assignment?.Id;
        }

        public async Task<string> AddAsync(CommitteeAssignmentCreateDto dto)
        {
            // Validate LecturerId not empty/whitespace
            if (string.IsNullOrWhiteSpace(dto.LecturerId))
                throw new ArgumentException("Lecturer ID cannot be empty or whitespace", nameof(dto.LecturerId));

            // Validate Lecturer exists and is a Lecturer role
            var lecturer = await _uow.Lecturers.GetByIdAsync(dto.LecturerId);
            if (lecturer == null)
                throw new KeyNotFoundException($"Lecturer with ID '{dto.LecturerId}' not found or has been deleted");

            // Validate Council exists and is not soft deleted
            var council = await _uow.Councils.GetByIdAsync(dto.CouncilId);
            if (council == null)
                throw new KeyNotFoundException($"Council with ID {dto.CouncilId} not found or has been deleted");

            // Validate CouncilRole exists and is not soft deleted
            var councilRole = await _uow.CouncilRoles.GetByIdAsync(dto.CouncilRoleId);
            if (councilRole == null)
                throw new KeyNotFoundException($"Council Role with ID {dto.CouncilRoleId} not found or has been deleted");

            // Check for duplicate: Same lecturer cannot have the same role in the same council
            var existingAssignments = await _uow.CommitteeAssignments.GetByCouncilIdAsync(dto.CouncilId);
            var duplicate = existingAssignments.FirstOrDefault(a => 
                a.LecturerId == dto.LecturerId && 
                a.CouncilRoleId == dto.CouncilRoleId && 
                !a.IsDeleted);
            
            if (duplicate != null)
                throw new InvalidOperationException(
                    $"Lecturer '{lecturer.FullName}' is already assigned as '{councilRole.RoleName}' in Council {dto.CouncilId}");

            // Validate Council is active
            if (!council.IsActive)
                throw new InvalidOperationException($"Cannot assign to inactive Council {dto.CouncilId}. Please activate the council first.");

            // ✅ NEW: Check for schedule conflicts with other councils the lecturer is assigned to
            await ValidateLecturerScheduleConflictAsync(dto.LecturerId, dto.CouncilId, lecturer.FullName);

            var entity = _mapper.Map<CommitteeAssignment>(dto);
            entity.Id = Guid.NewGuid().ToString();
            entity.IsDeleted = false;
            
            await _uow.CommitteeAssignments.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(string id, CommitteeAssignmentUpdateDto dto)
        {
            // Validate ID
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Committee Assignment ID cannot be null or empty", nameof(id));

            // Check if CommitteeAssignment exists
            var existing = await _uow.CommitteeAssignments.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Validate LecturerId not empty/whitespace
            if (string.IsNullOrWhiteSpace(dto.LecturerId))
                throw new ArgumentException("Lecturer ID cannot be empty or whitespace", nameof(dto.LecturerId));

            // Validate new Lecturer exists
            var lecturer = await _uow.Lecturers.GetByIdAsync(dto.LecturerId);
            if (lecturer == null)
                throw new KeyNotFoundException($"Lecturer with ID '{dto.LecturerId}' not found or has been deleted");

            // Validate new Council exists
            var council = await _uow.Councils.GetByIdAsync(dto.CouncilId);
            if (council == null)
                throw new KeyNotFoundException($"Council with ID {dto.CouncilId} not found or has been deleted");

            // Validate new CouncilRole exists
            var councilRole = await _uow.CouncilRoles.GetByIdAsync(dto.CouncilRoleId);
            if (councilRole == null)
                throw new KeyNotFoundException($"Council Role with ID {dto.CouncilRoleId} not found or has been deleted");

            // Check if changing to a different combination
            if (existing.LecturerId != dto.LecturerId || 
                existing.CouncilId != dto.CouncilId || 
                existing.CouncilRoleId != dto.CouncilRoleId)
            {
                // Check for duplicate with the new combination
                var existingAssignments = await _uow.CommitteeAssignments.GetByCouncilIdAsync(dto.CouncilId);
                var duplicate = existingAssignments.FirstOrDefault(a => 
                    a.Id != id &&
                    a.LecturerId == dto.LecturerId && 
                    a.CouncilRoleId == dto.CouncilRoleId && 
                    !a.IsDeleted);
                
                if (duplicate != null)
                    throw new InvalidOperationException(
                        $"Lecturer '{lecturer.FullName}' is already assigned as '{councilRole.RoleName}' in Council {dto.CouncilId}");
            }

            // Validate new Council is active
            if (!council.IsActive)
                throw new InvalidOperationException($"Cannot update to inactive Council {dto.CouncilId}. Please activate the council first.");

            // Check if CommitteeAssignment is being used in active ProjectTasks
            var tasksAsAssigner = await _uow.ProjectTasks.GetByAssignerAsync(id);
            var tasksAsAssignee = await _uow.ProjectTasks.GetByAssigneeAsync(id);
            var hasActiveTasks = tasksAsAssigner.Any() || tasksAsAssignee.Any();
            
            if (hasActiveTasks && (existing.LecturerId != dto.LecturerId || existing.CouncilId != dto.CouncilId))
                throw new InvalidOperationException(
                    $"Cannot change Lecturer or Council for this assignment because it has associated project tasks. Please remove or reassign the tasks first.");

            // ✅ NEW: Check for schedule conflicts when changing lecturer or council
            if (existing.LecturerId != dto.LecturerId || existing.CouncilId != dto.CouncilId)
            {
                await ValidateLecturerScheduleConflictAsync(dto.LecturerId, dto.CouncilId, lecturer.FullName, id);
            }

            _mapper.Map(dto, existing);
            await _uow.CommitteeAssignments.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await SoftDeleteAsync(id);
        }

        public async Task<bool> SoftDeleteAsync(string id)
        {
            // Validate ID
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Committee Assignment ID cannot be null or empty", nameof(id));

            var existing = await _uow.CommitteeAssignments.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Check if CommitteeAssignment is being used in MemberNotes
            // Use GetAllAsync and filter (since GetByCommitteeAssignmentIdAsync doesn't exist)
            var allMemberNotes = await _uow.MemberNotes.GetAllAsync();
            var memberNotes = allMemberNotes.Where(mn => mn.CommitteeAssignmentId == id);
            
            if (memberNotes.Any())
                throw new InvalidOperationException(
                    $"Cannot delete this committee assignment because it has {memberNotes.Count()} associated member note(s). Please remove the notes first.");

            // Check if CommitteeAssignment is being used in ProjectTasks (as assigner or assignee)
            var tasksAsAssigner = await _uow.ProjectTasks.GetByAssignerAsync(id);
            var tasksAsAssignee = await _uow.ProjectTasks.GetByAssigneeAsync(id);
            var totalTasks = tasksAsAssigner.Count() + tasksAsAssignee.Count();
            
            if (totalTasks > 0)
                throw new InvalidOperationException(
                    $"Cannot delete this committee assignment because it has {totalTasks} associated project task(s). Please remove or reassign the tasks first.");

            // Check if CommitteeAssignment is being used in Scores (as evaluator)
            var scores = await _uow.Scores.GetByEvaluatorIdAsync(existing.LecturerId);
            var scoresInCouncil = scores.Where(s => {
                // Check if score is related to a session that uses this council
                var session = _uow.DefenseSessions.GetByIdAsync(s.SessionId).Result;
                return session != null && session.CouncilId == existing.CouncilId;
            }).ToList();

            if (scoresInCouncil.Any())
                throw new InvalidOperationException(
                    $"Cannot delete this committee assignment because the lecturer has {scoresInCouncil.Count} score evaluation(s) in this council. Please remove the scores first.");

            await _uow.CommitteeAssignments.SoftDeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(string id)
        {
            // Validate ID
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Committee Assignment ID cannot be null or empty", nameof(id));

            var existing = await _uow.CommitteeAssignments.GetByIdAsync(id, includeDeleted: true);
            if (existing == null) 
                return false;

            // Validate Lecturer still exists and not deleted
            var lecturer = await _uow.Lecturers.GetByIdAsync(existing.LecturerId);
            if (lecturer == null)
                throw new InvalidOperationException(
                    $"Cannot restore this assignment because Lecturer with ID '{existing.LecturerId}' no longer exists or has been deleted");

            // Validate Council still exists
            var council = await _uow.Councils.GetByIdAsync(existing.CouncilId);
            if (council == null)
                throw new InvalidOperationException(
                    $"Cannot restore this assignment because Council with ID {existing.CouncilId} no longer exists or has been deleted");

            // Validate CouncilRole still exists
            var councilRole = await _uow.CouncilRoles.GetByIdAsync(existing.CouncilRoleId);
            if (councilRole == null)
                throw new InvalidOperationException(
                    $"Cannot restore this assignment because Council Role with ID {existing.CouncilRoleId} no longer exists or has been deleted");

            // Check if there's an active duplicate (same lecturer, same role, same council)
            var existingAssignments = await _uow.CommitteeAssignments.GetByCouncilIdAsync(existing.CouncilId);
            var duplicate = existingAssignments.FirstOrDefault(a => 
                a.Id != id &&
                a.LecturerId == existing.LecturerId && 
                a.CouncilRoleId == existing.CouncilRoleId && 
                !a.IsDeleted);
            
            if (duplicate != null)
                throw new InvalidOperationException(
                    $"Cannot restore this assignment because Lecturer '{lecturer.FullName}' is already assigned as '{councilRole.RoleName}' in Council {existing.CouncilId}");

            // Validate Council is active
            if (!council.IsActive)
                throw new InvalidOperationException(
                    $"Cannot restore this assignment because Council {existing.CouncilId} is inactive. Please activate the council first.");

            // ✅ NEW: Check for schedule conflicts when restoring
            await ValidateLecturerScheduleConflictAsync(existing.LecturerId, existing.CouncilId, lecturer.FullName, id);

            await _uow.CommitteeAssignments.RestoreAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Validates that a lecturer doesn't have schedule conflicts with defense sessions
        /// from other councils they are already assigned to.
        /// </summary>
        /// <param name="lecturerId">The lecturer ID to check</param>
        /// <param name="newCouncilId">The council ID being assigned to</param>
        /// <param name="lecturerName">The lecturer's name for error messages</param>
        /// <param name="excludeAssignmentId">Optional: Assignment ID to exclude (for updates)</param>
        private async Task ValidateLecturerScheduleConflictAsync(
            string lecturerId, 
            int newCouncilId, 
            string lecturerName, 
            string? excludeAssignmentId = null)
        {
            // Get all councils the lecturer is currently assigned to (excluding the one being updated)
            var lecturerAssignments = await _uow.CommitteeAssignments.GetByLecturerIdAsync(lecturerId);
            var existingCouncilIds = lecturerAssignments
                .Where(a => !a.IsDeleted && 
                           a.CouncilId != newCouncilId &&
                           (excludeAssignmentId == null || a.Id != excludeAssignmentId))
                .Select(a => a.CouncilId)
                .Distinct()
                .ToList();

            if (!existingCouncilIds.Any())
                return; // No existing assignments to other councils, no conflict possible

            // Get all active defense sessions for the new council
            var newCouncilSessions = await _uow.DefenseSessions.Query()
                .Where(s => s.CouncilId == newCouncilId && 
                           !s.IsDeleted && 
                           s.Status != "Cancelled" && 
                           s.Status != "Completed")
                .ToListAsync();

            if (!newCouncilSessions.Any())
                return; // No active sessions in the new council, no conflict possible

            // Get all active defense sessions for the lecturer's existing councils
            var existingSessions = await _uow.DefenseSessions.Query()
                .Where(s => existingCouncilIds.Contains(s.CouncilId) && 
                           !s.IsDeleted && 
                           s.Status != "Cancelled" && 
                           s.Status != "Completed")
                .ToListAsync();

            if (!existingSessions.Any())
                return; // No active sessions in existing councils, no conflict possible

            // Check for time overlaps between new council sessions and existing council sessions
            foreach (var newSession in newCouncilSessions)
            {
                foreach (var existingSession in existingSessions)
                {
                    // Check if same date
                    if (newSession.DefenseDate.Date == existingSession.DefenseDate.Date)
                    {
                        // Check if time ranges overlap
                        if (TimeRangesOverlap(newSession.StartTime, newSession.EndTime, 
                                              existingSession.StartTime, existingSession.EndTime))
                        {
                            throw new InvalidOperationException(
                                $"Cannot assign lecturer '{lecturerName}' to Council {newCouncilId}. " +
                                $"Schedule conflict detected: " +
                                $"Council {newCouncilId} has a defense session on {newSession.DefenseDate:dd/MM/yyyy} " +
                                $"({newSession.StartTime:hh\\:mm}-{newSession.EndTime:hh\\:mm}), " +
                                $"but the lecturer is already assigned to Council {existingSession.CouncilId} " +
                                $"which has a session at the same time " +
                                $"({existingSession.StartTime:hh\\:mm}-{existingSession.EndTime:hh\\:mm}).");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if two time ranges overlap
        /// </summary>
        private bool TimeRangesOverlap(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
        {
            return start1 < end2 && end1 > start2;
        }
    }
}
