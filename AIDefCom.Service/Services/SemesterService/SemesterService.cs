using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Semester;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.SemesterService
{
    public class SemesterService : ISemesterService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public SemesterService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SemesterReadDto>> GetAllAsync(bool includeDeleted = false)
        {
            var list = await _uow.Semesters.GetAllAsync(includeDeleted);
            return list.Select(s => new SemesterReadDto
            {
                Id = s.Id,
                SemesterName = s.SemesterName,
                Year = s.Year,
                StartDate = s.StartDate,
                EndDate = s.EndDate
            });
        }

        public async Task<SemesterReadDto?> GetByIdAsync(int id)
        {
            var s = await _uow.Semesters.GetByIdAsync(id);
            return s == null ? null : new SemesterReadDto
            {
                Id = s.Id,
                SemesterName = s.SemesterName,
                Year = s.Year,
                StartDate = s.StartDate,
                EndDate = s.EndDate
            };
        }

        public async Task<IEnumerable<SemesterReadDto>> GetByMajorIdAsync(int majorId)
        {
            // Semester không còn MajorId, return empty
            return await Task.FromResult(Enumerable.Empty<SemesterReadDto>());
        }

        public async Task<int> AddAsync(SemesterCreateDto dto)
        {
            // robust duplicate check
            if (await _uow.Semesters.ExistsByNameAsync(dto.SemesterName, dto.Year, 0))
                throw new InvalidOperationException("Semester name already exists for the given year.");

            var entity = _mapper.Map<Semester>(dto);
            await _uow.Semesters.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, SemesterUpdateDto dto)
        {
            var existing = await _uow.Semesters.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _uow.Semesters.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await SoftDeleteAsync(id);
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var existing = await _uow.Semesters.GetByIdAsync(id);
            if (existing == null) return false;

            await _uow.Semesters.SoftDeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var existing = await _uow.Semesters.GetByIdAsync(id, includeDeleted: true);
            if (existing == null) return false;

            await _uow.Semesters.RestoreAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
