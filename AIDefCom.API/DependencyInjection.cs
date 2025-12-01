using AIDefCom.Repository.Repositories;
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
using AIDefCom.Repository.Repositories.RecordingRepository;
using AIDefCom.Repository.Repositories.TranscriptRepository;
using AIDefCom.Repository.Repositories.StudentGroupRepository;
using AIDefCom.Repository.Repositories.ScoreRepository;
using AIDefCom.Repository.Repositories.NoteRepository;
using AIDefCom.Repository.UnitOfWork;
using AIDefCom.Service.Services.AuthService;
using AIDefCom.Service.Services.CommitteeAssignmentService;
using AIDefCom.Service.Services.CouncilService;
using AIDefCom.Service.Services.DefenseSessionService;
using AIDefCom.Service.Services.EmailService;
using AIDefCom.Service.Services.GroupService;
using AIDefCom.Service.Services.LecturerService;
using AIDefCom.Service.Services.MajorRubricService;
using AIDefCom.Service.Services.MajorService;
using AIDefCom.Service.Services.MemberNoteService;
using AIDefCom.Service.Services.ProjectTaskService;
using AIDefCom.Service.Services.ReportService;
using AIDefCom.Service.Services.RubricService;
using AIDefCom.Service.Services.SemesterService;
using AIDefCom.Service.Services.StudentService;
using AIDefCom.Service.Services.TranscriptService;
using AIDefCom.Service.Services.TranscriptAnalysisService;
using AIDefCom.Service.Services.ScoreService;
using AIDefCom.Service.Services.ScoreNotification;
using AIDefCom.Service.Services.RedisCache;
using AIDefCom.Service.Services.NoteService;
using AIDefCom.Service.Services.DefenseReportService;
using Microsoft.Extensions.DependencyInjection;
using AIDefCom.Service.Services.RecordingService;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace AIDefCom.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Redis Configuration
            var redisConfig = configuration.GetSection("Redis");
            var redisConnectionString = $"{redisConfig["Host"]}:{redisConfig["Port"]},password={redisConfig["Password"]},ssl={redisConfig.GetValue<bool>("Ssl")},abortConnect=False";
            
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                return ConnectionMultiplexer.Connect(redisConnectionString);
            });
            
            services.AddScoped<IRedisCache, RedisCache>();

            // Repositories
            services.AddScoped<IAppUserRepository, AppUserRepository>();
            services.AddScoped<ILecturerRepository, LecturerRepository>();
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
            services.AddScoped<ICouncilRoleRepository, CouncilRoleRepository>();
            services.AddScoped<IStudentGroupRepository, StudentGroupRepository>();
            services.AddScoped<ICommitteeAssignmentRepository, CommitteeAssignmentRepository>();
            services.AddScoped<IProjectTaskRepository, ProjectTaskRepository>();
            services.AddScoped<IRecordingRepository, RecordingRepository>();
            services.AddScoped<ITranscriptRepository, TranscriptRepository>();
            services.AddScoped<IScoreRepository, ScoreRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // HttpClient for AI Services
            services.AddHttpClient<ITranscriptAnalysisService, TranscriptAnalysisService>();
            services.AddHttpClient<IDefenseReportService, DefenseReportService>();

            // SignalR Notification Services
            services.AddScoped<IScoreNotificationService, Services.ScoreNotificationService>();

            // Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IRubricService, RubricService>();
            services.AddScoped<IMajorService, MajorService>();
            services.AddScoped<IMajorRubricService, MajorRubricService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ISemesterService, SemesterService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<ILecturerService, LecturerService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IDefenseSessionService, DefenseSessionService>();
            services.AddScoped<IMemberNoteService, MemberNoteService>();
            services.AddScoped<ICouncilService, CouncilService>();
            services.AddScoped<ICommitteeAssignmentService, CommitteeAssignmentService>();
            services.AddScoped<IProjectTaskService, ProjectTaskService>();
            services.AddScoped<RecordingStorageService>();
            services.AddScoped<IRecordingService, RecordingService>();
            services.AddScoped<ITranscriptService, TranscriptService>();
            services.AddScoped<ITranscriptAnalysisService, TranscriptAnalysisService>();
            services.AddScoped<IScoreService, ScoreService>();
            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<IDefenseReportService, DefenseReportService>();
            services.AddScoped<AIDefCom.Service.Services.FileStorageService.IFileStorageService,
                              AIDefCom.Service.Services.FileStorageService.AzureBlobFileStorageService>();

            return services;
        }
    }
}
