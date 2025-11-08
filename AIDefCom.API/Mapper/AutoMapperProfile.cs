using AIDefCom.Repository.Entities;
using AIDefCom.Service.Dto.CommitteeAssignment;
using AIDefCom.Service.Dto.Council;
using AIDefCom.Service.Dto.DefenseSession;
using AIDefCom.Service.Dto.Group;
using AIDefCom.Service.Dto.Major;
using AIDefCom.Service.Dto.MajorRubric;
using AIDefCom.Service.Dto.MemberNote;
using AIDefCom.Service.Dto.ProjectTask;
using AIDefCom.Service.Dto.Report;
using AIDefCom.Service.Dto.Rubric;
using AIDefCom.Service.Dto.Semester;
using AIDefCom.Service.Dto.Student;
using AIDefCom.Service.Dto.Transcript;
using AutoMapper;

namespace AIDefCom.API.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Rubric, RubricReadDto>();
            CreateMap<RubricCreateDto, Rubric>();
            CreateMap<RubricUpdateDto, Rubric>();

            CreateMap<Major, MajorReadDto>();
            CreateMap<MajorCreateDto, Major>();
            CreateMap<MajorUpdateDto, Major>();

            CreateMap<MajorRubric, MajorRubricReadDto>()
                .ForMember(d => d.MajorName, o => o.MapFrom(s => s.Major!.MajorName))
                .ForMember(d => d.RubricName, o => o.MapFrom(s => s.Rubric!.RubricName));

            CreateMap<MajorRubricCreateDto, MajorRubric>();
            CreateMap<MajorRubricUpdateDto, MajorRubric>();

            CreateMap<Report, ReportReadDto>();
            CreateMap<ReportCreateDto, Report>();
            CreateMap<ReportUpdateDto, Report>();

            // Semester - không còn Major relationship
            CreateMap<Semester, SemesterReadDto>();
            CreateMap<SemesterCreateDto, Semester>();
            CreateMap<SemesterUpdateDto, Semester>();

            // Student - kế thừa AppUser, không còn User
            CreateMap<Student, StudentReadDto>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.FullName))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email));
            CreateMap<StudentCreateDto, Student>();
            CreateMap<StudentUpdateDto, Student>();

            CreateMap<Group, GroupReadDto>()
                .ForMember(dest => dest.SemesterName, opt => opt.MapFrom(src => src.Semester!.SemesterName));
            CreateMap<GroupCreateDto, Group>();
            CreateMap<GroupUpdateDto, Group>();

            CreateMap<DefenseSession, DefenseSessionReadDto>();
            CreateMap<DefenseSessionCreateDto, DefenseSession>();
            CreateMap<DefenseSessionUpdateDto, DefenseSession>();

            // MemberNote - CommitteeAssignment thay vì User
            CreateMap<MemberNote, MemberNoteReadDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.CommitteeAssignment!.Lecturer!.FullName))
                .ForMember(dest => dest.CommitteeAssignmentId, opt => opt.MapFrom(src => src.CommitteeAssignmentId));
            CreateMap<MemberNoteCreateDto, MemberNote>();
            CreateMap<MemberNoteUpdateDto, MemberNote>();

            CreateMap<Council, CouncilReadDto>();
            CreateMap<CouncilCreateDto, Council>();
            CreateMap<CouncilUpdateDto, Council>();

            // CommitteeAssignment - có CouncilRole
            CreateMap<CommitteeAssignment, CommitteeAssignmentReadDto>()
                .ForMember(dest => dest.LecturerName, opt => opt.MapFrom(src => src.Lecturer!.FullName))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.CouncilRole!.RoleName));
            CreateMap<CommitteeAssignmentCreateDto, CommitteeAssignment>();
            CreateMap<CommitteeAssignmentUpdateDto, CommitteeAssignment>();

            // ProjectTask - AssignedBy/To là CommitteeAssignment
            CreateMap<ProjectTask, ProjectTaskReadDto>()
                .ForMember(dest => dest.AssignedByName, opt => opt.MapFrom(src => src.AssignedBy!.Lecturer!.FullName))
                .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo!.Lecturer!.FullName));
            CreateMap<ProjectTaskCreateDto, ProjectTask>();
            CreateMap<ProjectTaskUpdateDto, ProjectTask>();

            // Transcript mappings
            CreateMap<Transcript, TranscriptReadDto>();
            CreateMap<TranscriptCreateDto, Transcript>();
            CreateMap<TranscriptUpdateDto, Transcript>();

            CreateMap<Lecturer, UserReadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "Lecturer"));

            CreateMap<Student, UserReadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "Student"));
        }
    }
}
