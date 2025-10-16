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

        public async Task<IEnumerable<MajorReadDto>> GetAllAsync()
        {
            var majors = await _uow.Majors.GetAllAsync();
            return _mapper.Map<IEnumerable<MajorReadDto>>(majors);
        }

        public async Task<MajorReadDto?> GetByIdAsync(int id)
        {
            var major = await _uow.Majors.GetByIdAsync(id);
            return major == null ? null : _mapper.Map<MajorReadDto>(major);
        }

        public async Task<int> AddAsync(MajorCreateDto dto)
        {
            if (await _uow.Majors.ExistsByNameAsync(dto.MajorName))
                throw new InvalidOperationException("Major name already exists.");

            var entity = _mapper.Map<Major>(dto);
            await _uow.Majors.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, MajorUpdateDto dto)
        {
            var existing = await _uow.Majors.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _uow.Majors.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _uow.Majors.GetByIdAsync(id);
            if (existing == null) return false;

            await _uow.Majors.DeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
