using AIDefCom.Repository.Entities;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Dto.ProjectTask;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.ProjectTaskService
{
    public class ProjectTaskService : IProjectTaskService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ProjectTaskService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProjectTaskReadDto>> GetAllAsync()
        {
            var list = await _uow.ProjectTasks.GetAllAsync();
            return list.Select(t => new ProjectTaskReadDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                AssignedById = t.AssignedById,
                AssignedByName = t.AssignedBy?.Lecturer?.FullName,
                AssignedToId = t.AssignedToId,
                AssignedToName = t.AssignedTo?.Lecturer?.FullName,
                RubricId = t.RubricId,
                Status = t.Status
            });
        }

        public async Task<ProjectTaskReadDto?> GetByIdAsync(int id)
        {
            var entity = await _uow.ProjectTasks.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<ProjectTaskReadDto>(entity);
        }

        public async Task<IEnumerable<ProjectTaskReadDto>> GetByAssignerAsync(string assignedById)
        {
            var list = await _uow.ProjectTasks.GetByAssignerAsync(assignedById);
            return _mapper.Map<IEnumerable<ProjectTaskReadDto>>(list);
        }

        public async Task<IEnumerable<ProjectTaskReadDto>> GetByAssigneeAsync(string assignedToId)
        {
            var list = await _uow.ProjectTasks.GetByAssigneeAsync(assignedToId);
            return _mapper.Map<IEnumerable<ProjectTaskReadDto>>(list);
        }

        public async Task<int> AddAsync(ProjectTaskCreateDto dto)
        {
            var entity = _mapper.Map<ProjectTask>(dto);
            await _uow.ProjectTasks.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(int id, ProjectTaskUpdateDto dto)
        {
            var existing = await _uow.ProjectTasks.GetByIdAsync(id);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _uow.ProjectTasks.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _uow.ProjectTasks.GetByIdAsync(id);
            if (entity == null) return false;

            await _uow.ProjectTasks.DeleteAsync(id);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
