using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.DefenseSession;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.DefenseSessionService
{
    public class DefenseSessionService : IDefenseSessionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public DefenseSessionService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DefenseSessionReadDto>> GetAllAsync()
        {
            var list = await _uow.DefenseSessions.GetAllAsync();
            return _mapper.Map<IEnumerable<DefenseSessionReadDto>>(list);
        }

        public async Task<DefenseSessionReadDto?> GetByIdAsync(int id)
        {
            var entity = await _uow.DefenseSessions.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<DefenseSessionReadDto>(entity);
        }

        public async Task<IEnumerable<DefenseSessionReadDto>> GetByGroupIdAsync(string groupId)
        {
            var list = await _uow.DefenseSessions.GetByGroupIdAsync(groupId);
            return _mapper.Map<IEnumerable<DefenseSessionReadDto>>(list);
        }

        public async Task<int> AddAsync(DefenseSessionCreateDto dto)
        {
            var entity = _mapper.Map<DefenseSession>(dto);
            entity.CreatedAt = DateTime.UtcNow;

            await _uow.DefenseSessions.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, DefenseSessionUpdateDto dto)
        {
            var existing = await _uow.DefenseSessions.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _uow.DefenseSessions.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _uow.DefenseSessions.GetByIdAsync(id);
            if (entity == null) return false;

            await _uow.DefenseSessions.DeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
