using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.DefenseSession;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.DefenseSessionService
{
    public class DefenseSessionService : IDefenseSessionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public DefenseSessionService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // ------------------ CRUD ------------------

        public async Task<IEnumerable<DefenseSessionReadDto>> GetAllAsync(bool includeDeleted = false)
        {
            var list = await _uow.DefenseSessions.GetAllAsync(includeDeleted);
            return _mapper.Map<IEnumerable<DefenseSessionReadDto>>(list);
        }

        public async Task<DefenseSessionReadDto?> GetByIdAsync(int id)
        {
            var entity = await _uow.DefenseSessions.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<DefenseSessionReadDto>(entity);
        }

        public async Task<IEnumerable<DefenseSessionReadDto>> GetByGroupIdAsync(string groupId)
        {
            var list = await _uow.DefenseSessions.GetByGroupIdAsync(groupId);
            return _mapper.Map<IEnumerable<DefenseSessionReadDto>>(list);
        }

        public async Task<int> AddAsync(DefenseSessionCreateDto dto)
        {
            var council = await _uow.Councils.GetByIdAsync(dto.CouncilId);
            if (council == null)
                throw new ArgumentException($"Council with id {dto.CouncilId} not found.");

            var group = await _uow.Groups.GetByIdAsync(dto.GroupId);
            if (group == null)
                throw new ArgumentException($"Group with id {dto.GroupId} not found.");

            var entity = _mapper.Map<DefenseSession>(dto);
            entity.CreatedAt = DateTime.UtcNow;

            await _uow.DefenseSessions.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, DefenseSessionUpdateDto dto)
        {
            var existing = await _uow.DefenseSessions.GetByIdAsync(id);
            if (existing == null) return false;

            var council = await _uow.Councils.GetByIdAsync(dto.CouncilId);
            if (council == null) return false;

            _mapper.Map(dto, existing);
            await _uow.DefenseSessions.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await SoftDeleteAsync(id);
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var existing = await _uow.DefenseSessions.GetByIdAsync(id);
            if (existing == null) return false;

            await _uow.DefenseSessions.SoftDeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var existing = await _uow.DefenseSessions.GetByIdAsync(id, includeDeleted: true);
            if (existing == null) return false;

            await _uow.DefenseSessions.RestoreAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserReadDto>> GetUsersByDefenseSessionIdAsync(int defenseSessionId)
        {
            // 1️⃣ Lấy session
            var session = await _uow.DefenseSessions.GetByIdAsync(defenseSessionId);
            if (session == null)
                return Enumerable.Empty<UserReadDto>();

            var result = new List<UserReadDto>();

            var lecturerAssignments = await _uow.CommitteeAssignments.Query()
                .Include(ca => ca.Lecturer)
                .Include(ca => ca.CouncilRole)
                .Where(ca => ca.CouncilId == session.CouncilId)
                .ToListAsync();

            foreach (var ca in lecturerAssignments)
            {
                if (ca.Lecturer != null)
                {
                    result.Add(new UserReadDto
                    {
                        Id = ca.Lecturer.Id,
                        FullName = ca.Lecturer.FullName,
                        Email = ca.Lecturer.Email ?? string.Empty,
                        Role = ca.CouncilRole?.RoleName ?? "Committee Member"
                    });
                }
            }

            var studentGroups = await _uow.StudentGroups.Query()
                .Include(sg => sg.Student)
                .Where(sg => sg.GroupId == session.GroupId)
                .ToListAsync();

            foreach (var sg in studentGroups)
            {
                if (sg.Student != null)
                {
                    result.Add(new UserReadDto
                    {
                        Id = sg.Student.Id,
                        FullName = sg.Student.FullName,
                        Email = sg.Student.Email ?? string.Empty,
                        Role = sg.GroupRole ?? "Student"
                    });
                }
            }

            return result;
        }
    }
}
