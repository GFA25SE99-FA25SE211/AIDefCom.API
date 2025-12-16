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

        // Student presentations summary
        [JsonPropertyName("studentPresentations")]
        public List<StudentPresentationDto> StudentPresentations { get; set; } = new();

        // Questions and answers summary
        [JsonPropertyName("questionsAndAnswers")]
        public List<QuestionAnswerDto> QuestionsAndAnswers { get; set; } = new();

        // Overall summary from AI
        [JsonPropertyName("overallSummary")]
        public string? OverallSummary { get; set; }

        // Student performance evaluation
        [JsonPropertyName("studentPerformance")]
        public string? StudentPerformance { get; set; }

        // Main discussion topics
        [JsonPropertyName("discussionFocus")]
        public string? DiscussionFocus { get; set; }
    }

    /// <summary>
    /// Individual student presentation (AI analyzed)
    /// </summary>
    public class StudentPresentationDto
    {
        [JsonPropertyName("studentName")]
        public string StudentName { get; set; } = string.Empty;

        [JsonPropertyName("presentationContent")]
        public List<string> PresentationContent { get; set; } = new();

        [JsonPropertyName("presentationQuality")]
        public string? PresentationQuality { get; set; } // Clear/Good/Needs improvement

        [JsonPropertyName("notes")]
        public string? Notes { get; set; } // Special notes about presentation
    }

    /// <summary>
    /// Question from council and answer from group (AI analyzed) - DETAILED VERSION
    /// </summary>
    public class QuestionAnswerDto
    {
        [JsonPropertyName("lecturer")]
        public string Lecturer { get; set; } = string.Empty;

        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("respondent")]
        public string Respondent { get; set; } = string.Empty; // Student name or "Group"

        [JsonPropertyName("answerContent")]
        public string AnswerContent { get; set; } = string.Empty; // Full summary of answer with arguments and evidence

        [JsonPropertyName("councilDiscussion")]
        public string? CouncilDiscussion { get; set; } // Council's reaction and discussion about the answer

        [JsonPropertyName("lecturerFollowUp")]
        public string? LecturerFollowUp { get; set; } // Follow-up from lecturer: explanation, correction, hints, or next question

        [JsonPropertyName("answerQuality")]
        public string AnswerQuality { get; set; } = string.Empty;
        // "Trả lời xuất sắc" - Excellent answer
        // "Trả lời tốt" - Good answer
        // "Trả lời khá" - Fair answer
        // "Trả lời yếu" - Weak answer
        // "Trả lời sai" - Wrong answer
        // "Không trả lời được" - Unable to answer
        // "Trả lời sau khi được gợi ý" - Answered after hints
        // "Không nhận được phản hồi" - No response received

        [JsonPropertyName("answerAttitude")]
        public string? AnswerAttitude { get; set; }
        // "Tự tin, lưu loát" - Confident, fluent
        // "Do dự, ngập ngừng" - Hesitant, stammering
        // "Cần suy nghĩ lâu" - Needed time to think
        // "Không chắc chắn" - Not confident
        // "N/A" - Not applicable

        [JsonPropertyName("finalOutcome")]
        public string? FinalOutcome { get; set; }
        // "Câu trả lời được chấp nhận" - Answer accepted
        // "Cần bổ sung thêm" - Needs more information
        // "Chưa thỏa đáng" - Not satisfactory
        // "Vấn đề vẫn còn tồn đọng" - Issue still pending

        [JsonPropertyName("additionalNotes")]
        public string? AdditionalNotes { get; set; } // Extra notes based on transcript
    }
}
