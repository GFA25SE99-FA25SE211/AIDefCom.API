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
using AIDefCom.Repository.Repositories.RecordingRepository;
using System.Threading.Tasks;

namespace AIDefCom.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IAppUserRepository AppUsers { get; }
        public IRubricRepository Rubrics { get; }
        public IMajorRepository Majors { get; }
        public IMajorRubricRepository MajorRubrics { get; }
        public IReportRepository Reports { get; }
        public ISemesterRepository Semesters { get; }
        public IStudentRepository Students { get; }
        public IGroupRepository Groups { get; }
        public IDefenseSessionRepository DefenseSessions { get; }
        public IMemberNoteRepository MemberNotes { get; }
        public ICouncilRepository Councils { get; }
        public ICommitteeAssignmentRepository CommitteeAssignments { get; }
        public IProjectTaskRepository ProjectTasks { get; }
        public IRecordingRepository Recordings { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            IAppUserRepository appUserRepository,
            IRubricRepository rubricRepository,
            IMajorRepository majorRepository,
            IMajorRubricRepository majorRubricRepository,
            IReportRepository reportRepository,
            ISemesterRepository semesterRepository,
            IStudentRepository studentRepository,
            IGroupRepository groupRepository,
            IDefenseSessionRepository defenseSessionRepository,
            IMemberNoteRepository memberNoteRepository,
            ICouncilRepository councilRepository,
            ICommitteeAssignmentRepository committeeAssignmentRepository,
            IProjectTaskRepository projectTaskRepository,
            IRecordingRepository recordingRepository)
        {
            _context = context;
            AppUsers = appUserRepository;
            Rubrics = rubricRepository;
            Majors = majorRepository;
            MajorRubrics = majorRubricRepository;
            Reports = reportRepository;
            Semesters = semesterRepository;
            Students = studentRepository;
            Groups = groupRepository;
            DefenseSessions = defenseSessionRepository;
            MemberNotes = memberNoteRepository;
            Councils = councilRepository;
            CommitteeAssignments = committeeAssignmentRepository;
            ProjectTasks = projectTaskRepository;
            Recordings = recordingRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
