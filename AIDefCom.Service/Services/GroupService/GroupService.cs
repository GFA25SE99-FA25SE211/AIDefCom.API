using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Group;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.GroupService
{
    public class GroupService : IGroupService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GroupService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GroupReadDto>> GetAllAsync(bool includeDeleted = false)
        {
            var list = await _uow.Groups.GetAllAsync(includeDeleted);
            return _mapper.Map<IEnumerable<GroupReadDto>>(list);
        }

        public async Task<GroupReadDto?> GetByIdAsync(string id)
        {
            // Validate ID
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Group ID cannot be null or empty", nameof(id));

            var entity = await _uow.Groups.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<GroupReadDto>(entity);
        }

        public async Task<IEnumerable<GroupReadDto>> GetBySemesterIdAsync(int semesterId)
        {
            // Validate SemesterId
            if (semesterId <= 0)
                throw new ArgumentException("Semester ID must be greater than 0", nameof(semesterId));

            var list = await _uow.Groups.GetBySemesterIdAsync(semesterId);
            return _mapper.Map<IEnumerable<GroupReadDto>>(list);
        }

        public async Task<string> AddAsync(GroupCreateDto dto)
        {
            // Validate Semester exists and not deleted
            var semester = await _uow.Semesters.GetByIdAsync(dto.SemesterId);
            if (semester == null)
                throw new KeyNotFoundException($"Semester with ID {dto.SemesterId} not found or has been deleted");

            // Validate Major exists and not deleted
            var major = await _uow.Majors.GetByIdAsync(dto.MajorId);
            if (major == null)
                throw new KeyNotFoundException($"Major with ID {dto.MajorId} not found or has been deleted");

            // Check for duplicate project code
            if (await _uow.Groups.ExistsByProjectCodeAsync(dto.ProjectCode))
                throw new InvalidOperationException($"Project code '{dto.ProjectCode}' already exists");

            // Validate semester date range (group should be created within semester period)
            var currentDate = DateTime.UtcNow.Date;
            if (currentDate < semester.StartDate.Date)
                throw new InvalidOperationException($"Cannot create group before semester start date ({semester.StartDate:yyyy-MM-dd})");
            
            if (currentDate > semester.EndDate.Date)
                throw new InvalidOperationException($"Cannot create group after semester end date ({semester.EndDate:yyyy-MM-dd})");

            var entity = _mapper.Map<Group>(dto);
            entity.Id = Guid.NewGuid().ToString();
            await _uow.Groups.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(string id, GroupUpdateDto dto)
        {
            // Validate ID
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Group ID cannot be null or empty", nameof(id));

            // Check if Group exists
            var existing = await _uow.Groups.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Validate Semester exists and not deleted
            var semester = await _uow.Semesters.GetByIdAsync(dto.SemesterId);
            if (semester == null)
                throw new KeyNotFoundException($"Semester with ID {dto.SemesterId} not found or has been deleted");

            // Validate Major exists and not deleted
            var major = await _uow.Majors.GetByIdAsync(dto.MajorId);
            if (major == null)
                throw new KeyNotFoundException($"Major with ID {dto.MajorId} not found or has been deleted");

            // Check for duplicate project code (if changed)
            if (existing.ProjectCode != dto.ProjectCode)
            {
                if (await _uow.Groups.ExistsByProjectCodeAsync(dto.ProjectCode))
                    throw new InvalidOperationException($"Project code '{dto.ProjectCode}' already exists");
            }

            // Validate status transition
            ValidateStatusTransition(existing.Status, dto.Status);

            _mapper.Map(dto, existing);
            await _uow.Groups.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTotalScoreAsync(string id, GroupTotalScoreUpdateDto dto)
        {
            // Validate ID
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Group ID cannot be null or empty", nameof(id));

            // Validate score precision (max 2 decimal places)
            if (Math.Round(dto.TotalScore, 2) != dto.TotalScore)
                throw new ArgumentException("Total score must have at most 2 decimal places");

            var existing = await _uow.Groups.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Only allow updating score for Completed groups
            if (existing.Status != "Completed")
                throw new InvalidOperationException($"Cannot update total score for group with status '{existing.Status}'. Only 'Completed' groups can have their scores updated.");

            existing.TotalScore = dto.TotalScore;
            await _uow.Groups.UpdateAsync(existing);
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
                throw new ArgumentException("Group ID cannot be null or empty", nameof(id));

            var existing = await _uow.Groups.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Check if group has defense sessions
            var defenseSessions = await _uow.DefenseSessions.GetByGroupIdAsync(id);
            if (defenseSessions.Any())
                throw new InvalidOperationException($"Cannot delete group '{existing.ProjectCode}' because it has defense sessions. Please remove defense sessions first.");

            // Check if group has students
            var studentGroups = await _uow.StudentGroups.GetByGroupIdAsync(id);
            if (studentGroups.Any())
                throw new InvalidOperationException($"Cannot delete group '{existing.ProjectCode}' because it has {studentGroups.Count()} student(s) assigned. Please remove students first.");

            // Check if group has member notes
            var memberNotes = await _uow.MemberNotes.GetByGroupIdAsync(id);
            if (memberNotes.Any())
                throw new InvalidOperationException($"Cannot delete group '{existing.ProjectCode}' because it has member notes. Please remove notes first.");

            await _uow.Groups.SoftDeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(string id)
        {
            // Validate ID
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Group ID cannot be null or empty", nameof(id));

            var existing = await _uow.Groups.GetByIdAsync(id, includeDeleted: true);
            if (existing == null) 
                return false;

            // Validate that Semester still exists and is not deleted
            var semester = await _uow.Semesters.GetByIdAsync(existing.SemesterId);
            if (semester == null)
                throw new InvalidOperationException($"Cannot restore group because Semester with ID {existing.SemesterId} no longer exists or has been deleted");

            // Validate that Major still exists and is not deleted
            var major = await _uow.Majors.GetByIdAsync(existing.MajorId);
            if (major == null)
                throw new InvalidOperationException($"Cannot restore group because Major with ID {existing.MajorId} no longer exists or has been deleted");

            // Check if project code already exists in active groups
            if (await _uow.Groups.ExistsByProjectCodeAsync(existing.ProjectCode))
                throw new InvalidOperationException($"Cannot restore group because project code '{existing.ProjectCode}' already exists in another active group");

            await _uow.Groups.RestoreAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Validates status transition rules
        /// </summary>
        private void ValidateStatusTransition(string currentStatus, string newStatus)
        {
            // Define valid transitions
            var validTransitions = new Dictionary<string, string[]>
            {
                ["Pending"] = new[] { "Active", "Cancelled" },
                ["Active"] = new[] { "Completed", "Inactive", "Cancelled" },
                ["Inactive"] = new[] { "Active", "Cancelled" },
                ["Completed"] = new string[] { }, // Cannot change from Completed
                ["Cancelled"] = new string[] { }  // Cannot change from Cancelled
            };

            if (currentStatus == newStatus)
                return; // No change

            if (!validTransitions.ContainsKey(currentStatus))
                throw new InvalidOperationException($"Unknown current status: {currentStatus}");

            if (!validTransitions[currentStatus].Contains(newStatus))
                throw new InvalidOperationException($"Invalid status transition from '{currentStatus}' to '{newStatus}'. Allowed transitions: {string.Join(", ", validTransitions[currentStatus])}");
        }
    }
}
