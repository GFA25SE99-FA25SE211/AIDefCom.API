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
            var entity = await _uow.Groups.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<GroupReadDto>(entity);
        }

        public async Task<IEnumerable<GroupReadDto>> GetBySemesterIdAsync(int semesterId)
        {
            var list = await _uow.Groups.GetBySemesterIdAsync(semesterId);
            return _mapper.Map<IEnumerable<GroupReadDto>>(list);
        }

        public async Task<string> AddAsync(GroupCreateDto dto)
        {
            if (await _uow.Groups.ExistsByProjectCodeAsync(dto.ProjectCode))
                throw new InvalidOperationException("Project code already exists.");

            var entity = _mapper.Map<Group>(dto);
            entity.Id = Guid.NewGuid().ToString();
            await _uow.Groups.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(string id, GroupUpdateDto dto)
        {
            var existing = await _uow.Groups.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
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
            var existing = await _uow.Groups.GetByIdAsync(id);
            if (existing == null) return false;

            await _uow.Groups.SoftDeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(string id)
        {
            var existing = await _uow.Groups.GetByIdAsync(id, includeDeleted: true);
            if (existing == null) return false;

            await _uow.Groups.RestoreAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
