using AIDefCom.Repository.Repositories.AppUserRepository;
using AIDefCom.Repository.Repositories.CommitteeAssignmentRepository;
using AIDefCom.Repository.Repositories.CouncilRepository;
using AIDefCom.Repository.Repositories.DefenseSessionRepository;
using AIDefCom.Repository.Repositories.GroupRepository;
using AIDefCom.Repository.Repositories.MajorRepository;
using AIDefCom.Repository.Repositories.MajorRubricRepository;
using AIDefCom.Repository.Repositories.MemberNoteRepository;
using AIDefCom.Repository.Repositories.ProjectTaskRepository;
using AIDefCom.Repository.Repositories.ReportRepository;
using AIDefCom.Repository.Repositories.RubricRepository;
using AIDefCom.Repository.Repositories.SemesterRepository;
using AIDefCom.Repository.Repositories.StudentRepository;
using System;
using System.Threading.Tasks;

namespace AIDefCom.Repository.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IMajorRepository Majors { get; }
        IAppUserRepository AppUsers { get; }
        IRubricRepository Rubrics { get; }
        IMajorRubricRepository MajorRubrics { get; }
        IReportRepository Reports { get; }
        ISemesterRepository Semesters { get; }
        IStudentRepository Students { get; }
        IGroupRepository Groups { get; }
        IDefenseSessionRepository DefenseSessions { get; }
        IMemberNoteRepository MemberNotes { get; }
        ICouncilRepository Councils { get; }
        ICommitteeAssignmentRepository CommitteeAssignments { get; }
        IProjectTaskRepository ProjectTasks { get; }
        Task<int> SaveChangesAsync();
    }
}
