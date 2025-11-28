using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.MajorRubric;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.MajorRubricService
{
    public class MajorRubricService : IMajorRubricService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public MajorRubricService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MajorRubricReadDto>> GetAllAsync(bool includeDeleted = false)
        {
            var list = await _uow.MajorRubrics.GetAllAsync(includeDeleted);
            return list.Select(x => new MajorRubricReadDto
            {
                Id = x.Id,
                MajorId = x.MajorId,
                MajorName = x.Major?.MajorName,
                RubricId = x.RubricId,
                RubricName = x.Rubric?.RubricName
            });
        }

        public async Task<IEnumerable<MajorRubricReadDto>> GetByMajorIdAsync(int majorId)
        {
            // Validate MajorId
            if (majorId <= 0)
                throw new ArgumentException("Major ID must be greater than 0", nameof(majorId));

            var list = await _uow.MajorRubrics.GetByMajorIdAsync(majorId);
            return list.Select(x => new MajorRubricReadDto
            {
                Id = x.Id,
                MajorId = x.MajorId,
                MajorName = x.Major?.MajorName,
                RubricId = x.RubricId,
                RubricName = x.Rubric?.RubricName
            });
        }

        public async Task<IEnumerable<MajorRubricReadDto>> GetByRubricIdAsync(int rubricId)
        {
            // Validate RubricId
            if (rubricId <= 0)
                throw new ArgumentException("Rubric ID must be greater than 0", nameof(rubricId));

            var list = await _uow.MajorRubrics.GetByRubricIdAsync(rubricId);
            return list.Select(x => new MajorRubricReadDto
            {
                Id = x.Id,
                MajorId = x.MajorId,
                MajorName = x.Major?.MajorName,
                RubricId = x.RubricId,
                RubricName = x.Rubric?.RubricName
            });
        }

        public async Task<int> AddAsync(MajorRubricCreateDto dto)
        {
            // Validate Major exists and not deleted
            var major = await _uow.Majors.GetByIdAsync(dto.MajorId);
            if (major == null)
                throw new KeyNotFoundException($"Major with ID {dto.MajorId} not found or has been deleted");

            // Validate Rubric exists and not deleted
            var rubric = await _uow.Rubrics.GetByIdAsync(dto.RubricId);
            if (rubric == null)
                throw new KeyNotFoundException($"Rubric with ID {dto.RubricId} not found or has been deleted");

            // Check for duplicate
            if (await _uow.MajorRubrics.ExistsAsync(dto.MajorId, dto.RubricId))
                throw new InvalidOperationException($"The association between Major '{major.MajorName}' and Rubric '{rubric.RubricName}' already exists");

            var entity = _mapper.Map<MajorRubric>(dto);
            await _uow.MajorRubrics.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, MajorRubricUpdateDto dto)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("ID must be greater than 0", nameof(id));

            // Check if MajorRubric exists
            var existing = await _uow.MajorRubrics.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Validate new Major exists and not deleted
            var major = await _uow.Majors.GetByIdAsync(dto.MajorId);
            if (major == null)
                throw new KeyNotFoundException($"Major with ID {dto.MajorId} not found or has been deleted");

            // Validate new Rubric exists and not deleted
            var rubric = await _uow.Rubrics.GetByIdAsync(dto.RubricId);
            if (rubric == null)
                throw new KeyNotFoundException($"Rubric with ID {dto.RubricId} not found or has been deleted");

            // Check if changing to a new pair
            if ((existing.MajorId != dto.MajorId) || (existing.RubricId != dto.RubricId))
            {
                // Check for duplicate with the new pair
                var dup = await _uow.MajorRubrics.ExistsAsync(dto.MajorId, dto.RubricId);
                if (dup) 
                    throw new InvalidOperationException($"The association between Major '{major.MajorName}' and Rubric '{rubric.RubricName}' already exists");
            }

            existing.MajorId = dto.MajorId;
            existing.RubricId = dto.RubricId;

            await _uow.MajorRubrics.UpdateAsync(existing);
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
                throw new ArgumentException("ID must be greater than 0", nameof(id));

            var existing = await _uow.MajorRubrics.GetByIdAsync(id);
            if (existing == null) 
                return false;

            // Check if MajorRubric is being used in Scores
            var scoresUsingRubric = await _uow.Scores.GetByRubricIdAsync(existing.RubricId);
            var isUsedInScores = scoresUsingRubric.Any();
            
            if (isUsedInScores)
                throw new InvalidOperationException($"Cannot delete this Major-Rubric association because it is being used in score evaluations. Please remove related scores first.");

            await _uow.MajorRubrics.SoftDeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            // Validate ID
            if (id <= 0)
                throw new ArgumentException("ID must be greater than 0", nameof(id));

            var existing = await _uow.MajorRubrics.GetByIdAsync(id, includeDeleted: true);
            if (existing == null) 
                return false;

            // Validate that Major still exists and is not deleted
            var major = await _uow.Majors.GetByIdAsync(existing.MajorId);
            if (major == null)
                throw new InvalidOperationException($"Cannot restore this association because Major with ID {existing.MajorId} no longer exists or has been deleted");

            // Validate that Rubric still exists and is not deleted
            var rubric = await _uow.Rubrics.GetByIdAsync(existing.RubricId);
            if (rubric == null)
                throw new InvalidOperationException($"Cannot restore this association because Rubric with ID {existing.RubricId} no longer exists or has been deleted");

            // Check if the pair already exists (another record with same MajorId and RubricId that is not deleted)
            if (await _uow.MajorRubrics.ExistsAsync(existing.MajorId, existing.RubricId))
                throw new InvalidOperationException($"Cannot restore this association because an active association between Major '{major.MajorName}' and Rubric '{rubric.RubricName}' already exists");

            await _uow.MajorRubrics.RestoreAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
