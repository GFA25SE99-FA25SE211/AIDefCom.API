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
            if (await _uow.MajorRubrics.ExistsAsync(dto.MajorId, dto.RubricId))
                throw new InvalidOperationException("This Major–Rubric pair already exists.");

            var entity = _mapper.Map<MajorRubric>(dto);
            await _uow.MajorRubrics.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, MajorRubricUpdateDto dto)
        {
            var existing = await _uow.MajorRubrics.GetByIdAsync(id);
            if (existing == null) return false;

            // nếu đổi sang cặp mới, check trùng
            if ((existing.MajorId != dto.MajorId) || (existing.RubricId != dto.RubricId))
            {
                var dup = await _uow.MajorRubrics.ExistsAsync(dto.MajorId, dto.RubricId);
                if (dup) throw new InvalidOperationException("This Major–Rubric pair already exists.");
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
            var existing = await _uow.MajorRubrics.GetByIdAsync(id);
            if (existing == null) return false;

            await _uow.MajorRubrics.SoftDeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var existing = await _uow.MajorRubrics.GetByIdAsync(id, includeDeleted: true);
            if (existing == null) return false;

            await _uow.MajorRubrics.RestoreAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
