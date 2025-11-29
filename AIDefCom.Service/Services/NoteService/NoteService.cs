using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Note;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.NoteService
{
    public class NoteService : INoteService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public NoteService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NoteReadDto>> GetAllAsync()
        {
            var notes = await _uow.Notes.GetAllAsync();
            return notes.Select(n => _mapper.Map<NoteReadDto>(n));
        }

        public async Task<NoteReadDto?> GetByIdAsync(int id)
        {
            var note = await _uow.Notes.GetByIdAsync(id);
            return note == null ? null : _mapper.Map<NoteReadDto>(note);
        }

        public async Task<NoteReadDto?> GetBySessionIdAsync(int sessionId)
        {
            var note = await _uow.Notes.GetBySessionIdAsync(sessionId);
            return note == null ? null : _mapper.Map<NoteReadDto>(note);
        }

        public async Task<int> AddAsync(NoteCreateDto dto)
        {
            // one-to-one: ensure no note exists for session
            var existing = await _uow.Notes.GetBySessionIdAsync(dto.SessionId);
            if (existing != null)
                throw new System.InvalidOperationException("A note already exists for this session");

            // validate session exists
            var session = await _uow.DefenseSessions.GetByIdAsync(dto.SessionId);
            if (session == null) throw new System.ArgumentException("DefenseSession not found");

            var entity = _mapper.Map<Note>(dto);
            await _uow.Notes.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, NoteUpdateDto dto)
        {
            var existing = await _uow.Notes.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _uow.Notes.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _uow.Notes.GetByIdAsync(id);
            if (existing == null) return false;
            await _uow.Notes.DeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}