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
    .ForMember(d => d.MajorName, o => o.MapFrom(s => s.Major.MajorName))
    .ForMember(d => d.RubricName, o => o.MapFrom(s => s.Rubric.RubricName));

            CreateMap<MajorRubricCreateDto, MajorRubric>();
            CreateMap<MajorRubricUpdateDto, MajorRubric>();

            CreateMap<Report, ReportReadDto>();
            CreateMap<ReportCreateDto, Report>();
            CreateMap<ReportUpdateDto, Report>();

            CreateMap<Semester, SemesterReadDto>()
    .ForMember(dest => dest.MajorName, opt => opt.MapFrom(src => src.Major.MajorName));
            CreateMap<SemesterCreateDto, Semester>();
            CreateMap<SemesterUpdateDto, Semester>();

            CreateMap<Student, StudentReadDto>()
    .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User.FullName));
            CreateMap<StudentCreateDto, Student>();
            CreateMap<StudentUpdateDto, Student>();

            CreateMap<Group, GroupReadDto>()
    .ForMember(dest => dest.SemesterName, opt => opt.MapFrom(src => src.Semester.SemesterName));
            CreateMap<GroupCreateDto, Group>();
            CreateMap<GroupUpdateDto, Group>();

            CreateMap<DefenseSession, DefenseSessionReadDto>();
            CreateMap<DefenseSessionCreateDto, DefenseSession>();
            CreateMap<DefenseSessionUpdateDto, DefenseSession>();

            CreateMap<MemberNote, MemberNoteReadDto>()
    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName));
            CreateMap<MemberNoteCreateDto, MemberNote>();
            CreateMap<MemberNoteUpdateDto, MemberNote>();

            CreateMap<Council, CouncilReadDto>();
            CreateMap<CouncilCreateDto, Council>();
            CreateMap<CouncilUpdateDto, Council>();

            CreateMap<CommitteeAssignment, CommitteeAssignmentReadDto>()
    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName));
            CreateMap<CommitteeAssignmentCreateDto, CommitteeAssignment>();
            CreateMap<CommitteeAssignmentUpdateDto, CommitteeAssignment>();

            CreateMap<ProjectTask, ProjectTaskReadDto>()
    .ForMember(dest => dest.AssignedByName, opt => opt.MapFrom(src => src.AssignedBy.FullName))
    .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo.FullName));
            CreateMap<ProjectTaskCreateDto, ProjectTask>();
            CreateMap<ProjectTaskUpdateDto, ProjectTask>();

        }
    }
}
