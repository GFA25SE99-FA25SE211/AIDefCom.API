using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Rubric;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.RubricService
{
    public class RubricService : IRubricService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public RubricService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RubricReadDto>> GetAllAsync()
        {
            var rubrics = await _uow.Rubrics.GetAllAsync();
            return _mapper.Map<IEnumerable<RubricReadDto>>(rubrics);
        }

        public async Task<RubricReadDto?> GetByIdAsync(int id)
        {
            var rubric = await _uow.Rubrics.GetByIdAsync(id);
            return rubric == null ? null : _mapper.Map<RubricReadDto>(rubric);
        }

        public async Task<int> AddAsync(RubricCreateDto dto)
        {
            if (await _uow.Rubrics.ExistsByNameAsync(dto.RubricName))
                throw new InvalidOperationException("Rubric name already exists.");

            var rubric = _mapper.Map<Rubric>(dto);
            rubric.CreatedAt = DateTime.UtcNow;

            await _uow.Rubrics.AddAsync(rubric);
            await _uow.SaveChangesAsync();

            return rubric.Id;
        }

        public async Task<bool> UpdateAsync(int id, RubricUpdateDto dto)
        {
            var existing = await _uow.Rubrics.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);

            await _uow.Rubrics.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rubric = await _uow.Rubrics.GetByIdAsync(id);
            if (rubric == null) return false;

            await _uow.Rubrics.DeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
