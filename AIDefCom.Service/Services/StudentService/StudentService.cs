using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.Student;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.StudentService
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public StudentService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<StudentReadDto>> GetAllAsync()
        {
            var list = await _uow.Students.GetAllAsync();
            return list.Select(s => new StudentReadDto
            {
                Id = s.Id,
                UserId = s.UserId,
                UserName = s.User?.FullName,
                GroupId = s.GroupId,
                DateOfBirth = s.DateOfBirth,
                Gender = s.Gender,
                Role = s.Role
            });
        }

        public async Task<StudentReadDto?> GetByIdAsync(string id)
        {
            var s = await _uow.Students.GetByIdAsync(id);
            return s == null ? null : _mapper.Map<StudentReadDto>(s);
        }

        public async Task<IEnumerable<StudentReadDto>> GetByGroupIdAsync(string groupId)
        {
            var list = await _uow.Students.GetByGroupIdAsync(groupId);
            return _mapper.Map<IEnumerable<StudentReadDto>>(list);
        }

        public async Task<IEnumerable<StudentReadDto>> GetByUserIdAsync(string userId)
        {
            var list = await _uow.Students.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<StudentReadDto>>(list);
        }

        public async Task<string> AddAsync(StudentCreateDto dto)
        {
            var entity = _mapper.Map<Student>(dto);
            entity.Id = Guid.NewGuid().ToString();
            await _uow.Students.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(string id, StudentUpdateDto dto)
        {
            var existing = await _uow.Students.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _uow.Students.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _uow.Students.GetByIdAsync(id);
            if (entity == null) return false;

            await _uow.Students.DeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
