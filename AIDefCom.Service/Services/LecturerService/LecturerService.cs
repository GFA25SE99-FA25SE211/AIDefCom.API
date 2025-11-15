using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Lecturer;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.LecturerService
{
    public class LecturerService : ILecturerService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public LecturerService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LecturerReadDto>> GetAllAsync()
        {
            var list = await _uow.Lecturers.GetAllAsync();
            return list.Select(l => new LecturerReadDto
            {
                Id = l.Id,
                FullName = l.FullName,
                Email = l.Email,
                PhoneNumber = l.PhoneNumber,
                DateOfBirth = l.DateOfBirth,
                Gender = l.Gender,
                Department = l.Department,
                AcademicRank = l.AcademicRank,
                Degree = l.Degree
            });
        }

        public async Task<LecturerReadDto?> GetByIdAsync(string id)
        {
            var lecturer = await _uow.Lecturers.GetByIdAsync(id);
            return lecturer == null ? null : _mapper.Map<LecturerReadDto>(lecturer);
        }

        public async Task<IEnumerable<LecturerReadDto>> GetByDepartmentAsync(string department)
        {
            var list = await _uow.Lecturers.GetByDepartmentAsync(department);
            return _mapper.Map<IEnumerable<LecturerReadDto>>(list);
        }

        public async Task<IEnumerable<LecturerReadDto>> GetByAcademicRankAsync(string academicRank)
        {
            var list = await _uow.Lecturers.GetByAcademicRankAsync(academicRank);
            return _mapper.Map<IEnumerable<LecturerReadDto>>(list);
        }

        public async Task<string> AddAsync(LecturerCreateDto dto)
        {
            var entity = _mapper.Map<Lecturer>(dto);
            // Id should be provided from dto (from AppUser creation)
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }
            await _uow.Lecturers.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(string id, LecturerUpdateDto dto)
        {
            var existing = await _uow.Lecturers.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _uow.Lecturers.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _uow.Lecturers.GetByIdAsync(id);
            if (entity == null) return false;

            await _uow.Lecturers.DeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
