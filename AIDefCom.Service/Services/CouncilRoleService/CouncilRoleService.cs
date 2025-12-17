using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.CouncilRole;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.CouncilRoleService
{
    public class CouncilRoleService : ICouncilRoleService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CouncilRoleService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CouncilRoleReadDto>> GetAllAsync(bool includeDeleted = false)
        {
            var list = await _uow.CouncilRoles.GetAllAsync(includeDeleted);
            return _mapper.Map<IEnumerable<CouncilRoleReadDto>>(list);
        }

        public async Task<CouncilRoleReadDto?> GetByIdAsync(int id)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("Council role ID must be greater than 0", nameof(id));

            var entity = await _uow.CouncilRoles.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<CouncilRoleReadDto>(entity);
        }

        public async Task<CouncilRoleReadDto?> GetByRoleNameAsync(string roleName)
        {
            // Validate RoleName
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be null or empty", nameof(roleName));

            var entity = await _uow.CouncilRoles.GetByRoleNameAsync(roleName);
            return entity == null ? null : _mapper.Map<CouncilRoleReadDto>(entity);
        }

        public async Task<int> AddAsync(CouncilRoleCreateDto dto)
        {
            // Validate RoleName
            if (string.IsNullOrWhiteSpace(dto.RoleName))
                throw new ArgumentException("Role name cannot be empty");

            dto.RoleName = dto.RoleName.Trim();

            // Check for duplicate role name
            var existing = await _uow.CouncilRoles.GetByRoleNameAsync(dto.RoleName);
            if (existing != null)
                throw new InvalidOperationException($"Council role with name '{dto.RoleName}' already exists");

            var entity = _mapper.Map<CouncilRole>(dto);
            entity.IsDeleted = false;

            await _uow.CouncilRoles.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, CouncilRoleUpdateDto dto)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("Council role ID must be greater than 0", nameof(id));

            // Validate RoleName
            if (string.IsNullOrWhiteSpace(dto.RoleName))
                throw new ArgumentException("Role name cannot be empty");

            dto.RoleName = dto.RoleName.Trim();

            var existing = await _uow.CouncilRoles.GetByIdAsync(id);
            if (existing == null)
                return false;

            // Check for duplicate role name (exclude current role)
            var duplicate = await _uow.CouncilRoles.GetByRoleNameAsync(dto.RoleName);
            if (duplicate != null && duplicate.Id != id)
                throw new InvalidOperationException($"Council role with name '{dto.RoleName}' already exists");

            _mapper.Map(dto, existing);
            await _uow.CouncilRoles.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await SoftDeleteAsync(id);
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("Council role ID must be greater than 0", nameof(id));

            var existing = await _uow.CouncilRoles.GetByIdAsync(id);
            if (existing == null)
                return false;

            // Check if this role is being used in any committee assignments
            var assignments = await _uow.CommitteeAssignments.Query()
                .Where(ca => ca.CouncilRoleId == id && !ca.IsDeleted)
                .ToListAsync();

            if (assignments.Any())
                throw new InvalidOperationException($"Cannot delete council role because it is assigned to {assignments.Count} committee member(s). Please remove the assignments first.");

            await _uow.CouncilRoles.SoftDeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("Council role ID must be greater than 0", nameof(id));

            var existing = await _uow.CouncilRoles.GetByIdAsync(id, includeDeleted: true);
            if (existing == null)
                return false;

            // Check if already active
            if (!existing.IsDeleted)
                throw new InvalidOperationException($"Council role {id} is already active");

            await _uow.CouncilRoles.RestoreAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
