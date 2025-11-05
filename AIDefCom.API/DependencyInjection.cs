using AIDefCom.Repository.Repositories;
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
using AIDefCom.Repository.Repositories.RecordingRepository;
using AIDefCom.Repository.Repositories.TranscriptRepository;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Services.AuthService;
using AIDefCom.Service.Services.CommitteeAssignmentService;
using AIDefCom.Service.Services.CouncilService;
using AIDefCom.Service.Services.DefenseSessionService;
using AIDefCom.Service.Services.EmailService;
using AIDefCom.Service.Services.GroupService;
using AIDefCom.Service.Services.MajorRubricService;
using AIDefCom.Service.Services.MajorService;
using AIDefCom.Service.Services.MemberNoteService;
using AIDefCom.Service.Services.ProjectTaskService;
using AIDefCom.Service.Services.ReportService;
using AIDefCom.Service.Services.RubricService;
using AIDefCom.Service.Services.SemesterService;
using AIDefCom.Service.Services.StudentService;
using AIDefCom.Service.Services.TranscriptService;
using Microsoft.Extensions.DependencyInjection;
using AIDefCom.Service.Services.RecordingService;

namespace AIDefCom.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProjectServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IAppUserRepository, AppUserRepository>();
            services.AddScoped<IRubricRepository, RubricRepository>();
            services.AddScoped<IMajorRepository, MajorRepository>();
            services.AddScoped<IMajorRubricRepository, MajorRubricRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<ISemesterRepository, SemesterRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IDefenseSessionRepository, DefenseSessionRepository>();
            services.AddScoped<IMemberNoteRepository, MemberNoteRepository>();
            services.AddScoped<ICouncilRepository, CouncilRepository>();
            services.AddScoped<ICommitteeAssignmentRepository, CommitteeAssignmentRepository>();
            services.AddScoped<IProjectTaskRepository, ProjectTaskRepository>();
            services.AddScoped<IRecordingRepository, RecordingRepository>();
            services.AddScoped<ITranscriptRepository, TranscriptRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IRubricService, RubricService>();
            services.AddScoped<IMajorService, MajorService>();
            services.AddScoped<IMajorRubricService, MajorRubricService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ISemesterService, SemesterService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IDefenseSessionService, DefenseSessionService>();
            services.AddScoped<IMemberNoteService, MemberNoteService>();
            services.AddScoped<ICouncilService, CouncilService>();
            services.AddScoped<ICommitteeAssignmentService, CommitteeAssignmentService>();
            services.AddScoped<IProjectTaskService, ProjectTaskService>();
            services.AddScoped<RecordingStorageService>();
            services.AddScoped<IRecordingService, RecordingService>();
            services.AddScoped<ITranscriptService, TranscriptService>();

            return services;
        }
    }
}