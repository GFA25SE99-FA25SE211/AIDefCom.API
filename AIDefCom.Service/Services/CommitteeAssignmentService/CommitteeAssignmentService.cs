using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.CommitteeAssignment;
using AutoMapper;
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

        public async Task<IEnumerable<CommitteeAssignmentReadDto>> GetAllAsync()
        {
            var list = await _uow.CommitteeAssignments.GetAllAsync();
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

        public async Task<CommitteeAssignmentReadDto?> GetByIdAsync(int id)
        {
            var entity = await _uow.CommitteeAssignments.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<CommitteeAssignmentReadDto>(entity);
        }

        public async Task<IEnumerable<CommitteeAssignmentReadDto>> GetByCouncilIdAsync(int councilId)
        {
            var list = await _uow.CommitteeAssignments.GetByCouncilIdAsync(councilId);
            return _mapper.Map<IEnumerable<CommitteeAssignmentReadDto>>(list);
        }

        public async Task<IEnumerable<CommitteeAssignmentReadDto>> GetBySessionIdAsync(int sessionId)
        {
            var list = await _uow.CommitteeAssignments.GetBySessionIdAsync(sessionId);
            return _mapper.Map<IEnumerable<CommitteeAssignmentReadDto>>(list);
        }

        public async Task<IEnumerable<CommitteeAssignmentReadDto>> GetByLecturerIdAsync(string lecturerId)
        {
            var list = await _uow.CommitteeAssignments.GetByLecturerIdAsync(lecturerId);
            return _mapper.Map<IEnumerable<CommitteeAssignmentReadDto>>(list);
        }

        public async Task<string> AddAsync(CommitteeAssignmentCreateDto dto)
        {
            var entity = _mapper.Map<CommitteeAssignment>(dto);
            entity.Id = Guid.NewGuid().ToString();
            await _uow.CommitteeAssignments.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, CommitteeAssignmentUpdateDto dto)
        {
            var existing = await _uow.CommitteeAssignments.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _uow.CommitteeAssignments.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _uow.CommitteeAssignments.GetByIdAsync(id);
            if (existing == null) return false;

            await _uow.CommitteeAssignments.DeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
