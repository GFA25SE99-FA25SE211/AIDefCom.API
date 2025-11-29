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
using AIDefCom.Repository.Repositories.NoteRepository;
using System;
using System.Threading.Tasks;

namespace AIDefCom.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        // ------------------ Academic Data ------------------
        public IMajorRepository Majors { get; }
        public ISemesterRepository Semesters { get; }
        public IGroupRepository Groups { get; }
        public IStudentRepository Students { get; }
        public IStudentGroupRepository StudentGroups { get; }

        // ------------------ Rubrics & Evaluation ------------------
        public IRubricRepository Rubrics { get; }
        public IMajorRubricRepository MajorRubrics { get; }
        public IReportRepository Reports { get; }
        public IMemberNoteRepository MemberNotes { get; }
        public IScoreRepository Scores { get; }
        public INoteRepository Notes { get; }

        // ------------------ Defense & Committee ------------------
        public IDefenseSessionRepository DefenseSessions { get; }
        public ICouncilRepository Councils { get; }
        public ICouncilRoleRepository CouncilRoles { get; }
        public ICommitteeAssignmentRepository CommitteeAssignments { get; }

        // ------------------ Users & Projects ------------------
        public IAppUserRepository AppUsers { get; }
        public ILecturerRepository Lecturers { get; }
        public IProjectTaskRepository ProjectTasks { get; }

        // ------------------ Media & Transcripts ------------------
        public IRecordingRepository Recordings { get; }
        public ITranscriptRepository Transcripts { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            IAppUserRepository appUserRepository,
            ILecturerRepository lecturerRepository,
            IRubricRepository rubricRepository,
            IMajorRepository majorRepository,
            IMajorRubricRepository majorRubricRepository,
            IReportRepository reportRepository,
            ISemesterRepository semesterRepository,
            IStudentRepository studentRepository,
            IGroupRepository groupRepository,
            IStudentGroupRepository studentGroupRepository,
            IDefenseSessionRepository defenseSessionRepository,
            IMemberNoteRepository memberNoteRepository,
            ICouncilRepository councilRepository,
            ICouncilRoleRepository councilRoleRepository,
            ICommitteeAssignmentRepository committeeAssignmentRepository,
            IProjectTaskRepository projectTaskRepository,
            IRecordingRepository recordingRepository,
            ITranscriptRepository transcriptRepository,
            IScoreRepository scoreRepository,
            INoteRepository noteRepository)
        {
            _context = context;

            // Academic Data
            Majors = majorRepository;
            Semesters = semesterRepository;
            Groups = groupRepository;
            Students = studentRepository;
            StudentGroups = studentGroupRepository;

            // Rubrics & Evaluation
            Rubrics = rubricRepository;
            MajorRubrics = majorRubricRepository;
            Reports = reportRepository;
            MemberNotes = memberNoteRepository;
            Scores = scoreRepository;
            Notes = noteRepository;

            // Defense & Committee
            DefenseSessions = defenseSessionRepository;
            Councils = councilRepository;
            CouncilRoles = councilRoleRepository;
            CommitteeAssignments = committeeAssignmentRepository;

            // Users & Projects
            AppUsers = appUserRepository;
            Lecturers = lecturerRepository;
            ProjectTasks = projectTaskRepository;

            // Media & Transcripts
            Recordings = recordingRepository;
            Transcripts = transcriptRepository;
        }

        // ------------------ Core ------------------
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
