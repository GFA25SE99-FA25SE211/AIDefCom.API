using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Transcript;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.TranscriptService
{
    public class TranscriptService : ITranscriptService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public TranscriptService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TranscriptReadDto>> GetAllAsync()
        {
            var list = await _uow.Transcripts.GetAllAsync();
            return list.Select(t => _mapper.Map<TranscriptReadDto>(t));
        }

        public async Task<TranscriptReadDto?> GetByIdAsync(int id)
        {
            var entity = await _uow.Transcripts.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<TranscriptReadDto>(entity);
        }

        public async Task<IEnumerable<TranscriptReadDto>> GetBySessionIdAsync(int sessionId)
        {
            var list = await _uow.Transcripts.GetBySessionIdAsync(sessionId);
            return list.Select(t => _mapper.Map<TranscriptReadDto>(t));
        }

        public async Task<int> AddAsync(TranscriptCreateDto dto)
        {
            var entity = _mapper.Map<Transcript>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.Status = "Pending";
            
            await _uow.Transcripts.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, TranscriptUpdateDto dto)
        {
            var existing = await _uow.Transcripts.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _uow.Transcripts.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _uow.Transcripts.GetByIdAsync(id);
            if (existing == null) return false;

            await _uow.Transcripts.DeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
