using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Council;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.CouncilService
{
    public class CouncilService : ICouncilService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CouncilService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CouncilReadDto>> GetAllAsync(bool includeInactive = false)
        {
            var list = await _uow.Councils.GetAllAsync(includeInactive);
            return _mapper.Map<IEnumerable<CouncilReadDto>>(list);
        }

        public async Task<CouncilReadDto?> GetByIdAsync(int id)
        {
            var entity = await _uow.Councils.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<CouncilReadDto>(entity);
        }

        public async Task<int> AddAsync(CouncilCreateDto dto)
        {
            var entity = _mapper.Map<Council>(dto);
            entity.CreatedDate = DateTime.UtcNow;
            entity.IsActive = true;

            await _uow.Councils.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, CouncilUpdateDto dto)
        {
            var existing = await _uow.Councils.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _uow.Councils.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var existing = await _uow.Councils.GetByIdAsync(id);
            if (existing == null) return false;

            await _uow.Councils.SoftDeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var existing = await _uow.Councils.GetByIdAsync(id);
            if (existing == null) return false;

            await _uow.Councils.RestoreAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
