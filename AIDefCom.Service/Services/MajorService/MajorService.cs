using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Major;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.MajorService
{
    public class MajorService : IMajorService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public MajorService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MajorReadDto>> GetAllAsync(bool includeDeleted = false)
        {
            var majors = await _uow.Majors.GetAllAsync(includeDeleted);
            return _mapper.Map<IEnumerable<MajorReadDto>>(majors);
        }

        public async Task<MajorReadDto?> GetByIdAsync(int id)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("ID must be greater than 0", nameof(id));

            var major = await _uow.Majors.GetByIdAsync(id);
            return major == null ? null : _mapper.Map<MajorReadDto>(major);
        }

        public async Task<int> AddAsync(MajorCreateDto dto)
        {
            // Validate MajorName not empty (additional check beyond data annotations)
            if (string.IsNullOrWhiteSpace(dto.MajorName))
                throw new ArgumentException("Major name cannot be empty or whitespace", nameof(dto.MajorName));

            // Trim and normalize the name
            var normalizedName = dto.MajorName.Trim();

            // Check if major name already exists (case-insensitive)
            if (await _uow.Majors.ExistsByNameAsync(normalizedName))
                throw new InvalidOperationException($"A major with the name '{normalizedName}' already exists");

            var entity = _mapper.Map<Major>(dto);
            entity.MajorName = normalizedName; // Use normalized name
            
            await _uow.Majors.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, MajorUpdateDto dto)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("ID must be greater than 0", nameof(id));

            // Validate MajorName not empty (additional check beyond data annotations)
            if (string.IsNullOrWhiteSpace(dto.MajorName))
                throw new ArgumentException("Major name cannot be empty or whitespace", nameof(dto.MajorName));

            // Check if major exists
            var existing = await _uow.Majors.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Trim and normalize the name
            var normalizedName = dto.MajorName.Trim();

            // Check if the new name conflicts with another major (case-insensitive, excluding current)
            if (!existing.MajorName.Equals(normalizedName, StringComparison.OrdinalIgnoreCase))
            {
                if (await _uow.Majors.ExistsByNameAsync(normalizedName))
                    throw new InvalidOperationException($"A major with the name '{normalizedName}' already exists");
            }

            existing.MajorName = normalizedName;
            existing.Description = dto.Description;

            await _uow.Majors.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Deprecated - redirects to SoftDeleteAsync
            return await SoftDeleteAsync(id);
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("ID must be greater than 0", nameof(id));

            var existing = await _uow.Majors.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Check if Major is being used in Groups
            var groupsUsingMajor = await _uow.Groups.GetAllAsync();
            var hasGroups = groupsUsingMajor.Any(g => g.MajorId == id && !g.IsDeleted);
            
            if (hasGroups)
                throw new InvalidOperationException($"Cannot delete major '{existing.MajorName}' because it is being used by one or more groups. Please reassign or delete the groups first.");

            // Check if Major is being used in MajorRubrics
            var majorRubricsUsingMajor = await _uow.MajorRubrics.GetByMajorIdAsync(id);
            var hasMajorRubrics = majorRubricsUsingMajor.Any();
            
            if (hasMajorRubrics)
                throw new InvalidOperationException($"Cannot delete major '{existing.MajorName}' because it is associated with one or more rubrics. Please remove the rubric associations first.");

            // Check if Major is being used in Councils
            var councilsUsingMajor = await _uow.Councils.GetAllAsync(includeInactive: true);
            var hasCouncils = councilsUsingMajor.Any(c => c.MajorId == id);
            
            if (hasCouncils)
                throw new InvalidOperationException($"Cannot delete major '{existing.MajorName}' because it is being used by one or more councils. Please remove or reassign the councils first.");

            await _uow.Majors.SoftDeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("ID must be greater than 0", nameof(id));

            var existing = await _uow.Majors.GetByIdAsync(id, includeDeleted: true);
            if (existing == null) 
                return false;

            // Check if a major with the same name already exists (not deleted)
            if (await _uow.Majors.ExistsByNameAsync(existing.MajorName))
                throw new InvalidOperationException($"Cannot restore major '{existing.MajorName}' because a major with the same name already exists");

            await _uow.Majors.RestoreAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
