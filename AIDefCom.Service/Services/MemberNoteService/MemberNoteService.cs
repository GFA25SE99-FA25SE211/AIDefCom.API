using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.MemberNote;
using AIDefCom.Service.Services.CommitteeAssignmentService;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.MemberNoteService
{
    public class MemberNoteService : IMemberNoteService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ICommitteeAssignmentService _committeeAssignmentService;

        public MemberNoteService(IUnitOfWork uow, IMapper mapper, ICommitteeAssignmentService committeeAssignmentService)
        {
            _uow = uow;
            _mapper = mapper;
            _committeeAssignmentService = committeeAssignmentService;
        }

        public async Task<IEnumerable<MemberNoteReadDto>> GetAllAsync()
        {
            var list = await _uow.MemberNotes.GetAllAsync();
            return list.Select(n => new MemberNoteReadDto
            {
                Id = n.Id,
                CommitteeAssignmentId = n.CommitteeAssignmentId,
                UserName = n.CommitteeAssignment?.Lecturer?.FullName,
                SessionId = n.SessionId,
                NoteContent = n.NoteContent,
                CreatedAt = n.CreatedAt
            });
        }

        public async Task<MemberNoteReadDto?> GetByIdAsync(int id)
        {
            var note = await _uow.MemberNotes.GetByIdAsync(id);
            return note == null ? null : _mapper.Map<MemberNoteReadDto>(note);
        }

        public async Task<IEnumerable<MemberNoteReadDto>> GetBySessionIdAsync(int sessionId)
        {
            var list = await _uow.MemberNotes.GetBySessionIdAsync(sessionId);
            return _mapper.Map<IEnumerable<MemberNoteReadDto>>(list);
        }

        public async Task<IEnumerable<MemberNoteReadDto>> GetByUserIdAsync(string userId)
        {
            var list = await _uow.MemberNotes.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<MemberNoteReadDto>>(list);
        }

        public async Task<int> AddAsync(MemberNoteCreateDto dto)
        {
            // Get CommitteeAssignmentId from LecturerId and SessionId
            var committeeAssignmentId = await _committeeAssignmentService.GetIdByLecturerIdAndSessionIdAsync(dto.LecturerId, dto.SessionId);
            
            if (string.IsNullOrEmpty(committeeAssignmentId))
            {
                throw new KeyNotFoundException($"No committee assignment found for lecturer '{dto.LecturerId}' in session {dto.SessionId}. The lecturer must be assigned to the council for this defense session.");
            }

            var entity = new MemberNote
            {
                CommitteeAssignmentId = committeeAssignmentId,
                SessionId = dto.SessionId,
                NoteContent = dto.NoteContent,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.MemberNotes.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, MemberNoteUpdateDto dto)
        {
            var existing = await _uow.MemberNotes.GetByIdAsync(id);
            if (existing == null) return false;

            existing.NoteContent = dto.NoteContent;
            await _uow.MemberNotes.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _uow.MemberNotes.GetByIdAsync(id);
            if (entity == null) return false;

            await _uow.MemberNotes.DeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
