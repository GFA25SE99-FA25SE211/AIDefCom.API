using AIDefCom.Repository.Repositories.AppUserRepository;
using AIDefCom.Repository.Repositories.CommitteeAssignmentRepository;
using AIDefCom.Repository.Repositories.CouncilRepository;
using AIDefCom.Repository.Repositories.CouncilRoleRepository;
using AIDefCom.Repository.Repositories.DefenseSessionRepository;
using AIDefCom.Repository.Repositories.GroupRepository;
using AIDefCom.Repository.Repositories.LecturerRepository;
using AIDefCom.Repository.Repositories.MajorRepository;
using AIDefCom.Repository.Repositories.MajorRubricRepository;
using AIDefCom.Repository.Repositories.MemberNoteRepository;
using AIDefCom.Repository.Repositories.ProjectTaskRepository;
using AIDefCom.Repository.Repositories.ReportRepository;
using AIDefCom.Repository.Repositories.RubricRepository;
using AIDefCom.Repository.Repositories.SemesterRepository;
using AIDefCom.Repository.Repositories.StudentRepository;
using AIDefCom.Repository.Repositories.StudentGroupRepository;
using AIDefCom.Repository.Repositories.RecordingRepository;
using AIDefCom.Repository.Repositories.TranscriptRepository;
using AIDefCom.Repository.Repositories.ScoreRepository;
using System;
using System.Threading.Tasks;

namespace AIDefCom.Repository.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        // ------------------ Academic Data ------------------
        IMajorRepository Majors { get; }
        ISemesterRepository Semesters { get; }
        IGroupRepository Groups { get; }
        IStudentRepository Students { get; }
        IStudentGroupRepository StudentGroups { get; }

        // ------------------ Rubrics & Evaluation ------------------
        IRubricRepository Rubrics { get; }
        IMajorRubricRepository MajorRubrics { get; }
        IReportRepository Reports { get; }
        IMemberNoteRepository MemberNotes { get; }
        IScoreRepository Scores { get; }

        // ------------------ Defense & Committee ------------------
        IDefenseSessionRepository DefenseSessions { get; }
        ICouncilRepository Councils { get; }
        ICouncilRoleRepository CouncilRoles { get; }
        ICommitteeAssignmentRepository CommitteeAssignments { get; }

        // ------------------ Users & Projects ------------------
        IAppUserRepository AppUsers { get; }
        ILecturerRepository Lecturers { get; }
        IProjectTaskRepository ProjectTasks { get; }

        // ------------------ Media & Transcripts ------------------
        IRecordingRepository Recordings { get; }
        ITranscriptRepository Transcripts { get; }

        // ------------------ Core ------------------
        Task<int> SaveChangesAsync();
        Task<int> CompleteAsync();
    }
}
