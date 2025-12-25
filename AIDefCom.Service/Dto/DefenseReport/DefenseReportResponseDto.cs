using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AIDefCom.Service.Dto.DefenseReport
{
    /// <summary>
    /// Defense report response - Bien ban bao ve do an
    /// </summary>
    public class DefenseReportResponseDto
    {
        // PART 1: COUNCIL COMPOSITION
        [JsonPropertyName("councilInfo")]
        public CouncilInfoDto CouncilInfo { get; set; } = new();

        // PART 2: TIME AND LOCATION
        [JsonPropertyName("sessionInfo")]
        public SessionInfoDto SessionInfo { get; set; } = new();

        // PART 3: PROJECT AND GROUP INFO
        [JsonPropertyName("projectInfo")]
        public ProjectInfoDto ProjectInfo { get; set; } = new();

        // PART 4: DEFENSE PROGRESS (AI analyzed from transcript)
        [JsonPropertyName("defenseProgress")]
        public DefenseProgressDto DefenseProgress { get; set; } = new();
    }

    /// <summary>
    /// Council information
    /// </summary>
    public class CouncilInfoDto
    {
        [JsonPropertyName("councilId")]
        public int CouncilId { get; set; }

        [JsonPropertyName("majorName")]
        public string MajorName { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("members")]
        public List<CouncilMemberDto> Members { get; set; } = new();
    }

    /// <summary>
    /// Council member
    /// </summary>
    public class CouncilMemberDto
    {
        [JsonPropertyName("lecturerId")]
        public string LecturerId { get; set; } = string.Empty;

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty; // Chairman, Secretary, Member

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("department")]
        public string? Department { get; set; }

        [JsonPropertyName("academicRank")]
        public string? AcademicRank { get; set; }

        [JsonPropertyName("degree")]
        public string? Degree { get; set; }
    }

    /// <summary>
    /// Defense session information
    /// </summary>
    public class SessionInfoDto
    {
        [JsonPropertyName("defenseDate")]
        public DateTime DefenseDate { get; set; }

        [JsonPropertyName("startTime")]
        public TimeSpan StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public TimeSpan EndTime { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Project and student group information
    /// </summary>
    public class ProjectInfoDto
    {
        [JsonPropertyName("projectCode")]
        public string ProjectCode { get; set; } = string.Empty;

        [JsonPropertyName("topicTitleEN")]
        public string TopicTitleEN { get; set; } = string.Empty;

        [JsonPropertyName("topicTitleVN")]
        public string TopicTitleVN { get; set; } = string.Empty;

        [JsonPropertyName("semesterName")]
        public string SemesterName { get; set; } = string.Empty;

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("students")]
        public List<StudentInfoDto> Students { get; set; } = new();
    }

    /// <summary>
    /// Student information
    /// </summary>
    public class StudentInfoDto
    {
        [JsonPropertyName("studentId")]
        public string StudentId { get; set; } = string.Empty;

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("groupRole")]
        public string? GroupRole { get; set; } // Leader, Member
    }

    /// <summary>
    /// Defense progress (AI analyzed from transcript)
    /// </summary>
    public class DefenseProgressDto
    {
        [JsonPropertyName("actualStartTime")]
        public string? ActualStartTime { get; set; }

        [JsonPropertyName("actualEndTime")]
        public string? ActualEndTime { get; set; }

        /// <summary>
        /// Tóm tắt phần trình bày của nhóm/sinh viên
        /// </summary>
        [JsonPropertyName("studentPresentations")]
        public List<StudentPresentationDto> StudentPresentations { get; set; } = new();

        /// <summary>
        /// Câu hỏi từ Hội đồng và Nội dung trả lời từ Nhóm
        /// </summary>
        [JsonPropertyName("questionsAndAnswers")]
        public List<QuestionAnswerDto> QuestionsAndAnswers { get; set; } = new();

        // Overall summary from AI
        [JsonPropertyName("overallSummary")]
        public string? OverallSummary { get; set; }
    }

    /// <summary>
    /// Tóm tắt phần trình bày của sinh viên
    /// Format: Sinh viên | Nội dung trình bày
    /// </summary>
    public class StudentPresentationDto
    {
        /// <summary>
        /// Tên sinh viên
        /// </summary>
        [JsonPropertyName("studentName")]
        public string StudentName { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung trình bày - danh sách các ý chính (bullet points)
        /// </summary>
        [JsonPropertyName("presentationPoints")]
        public List<string> PresentationPoints { get; set; } = new();
    }

    /// <summary>
    /// Câu hỏi từ Hội đồng và Nội dung trả lời từ Nhóm
    /// Format: Câu hỏi từ Hội đồng | Nội dung trả lời từ Nhóm
    /// </summary>
    public class QuestionAnswerDto
    {
        /// <summary>
        /// Tên giảng viên đặt câu hỏi
        /// </summary>
        [JsonPropertyName("lecturerName")]
        public string LecturerName { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung câu hỏi (tóm tắt)
        /// </summary>
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        /// <summary>
        /// Tên sinh viên trả lời
        /// </summary>
        [JsonPropertyName("respondentName")]
        public string RespondentName { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung trả lời - danh sách các ý chính (bullet points)
        /// </summary>
        [JsonPropertyName("answerPoints")]
        public List<string> AnswerPoints { get; set; } = new();

        /// <summary>
        /// Nhận xét/thảo luận của Hội đồng về câu trả lời
        /// </summary>
        [JsonPropertyName("councilDiscussion")]
        public string? CouncilDiscussion { get; set; }
    }
}
