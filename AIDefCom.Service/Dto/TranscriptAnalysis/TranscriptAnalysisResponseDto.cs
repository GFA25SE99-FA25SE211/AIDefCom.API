using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AIDefCom.Service.Dto.TranscriptAnalysis
{
    public class TranscriptAnalysisResponseDto
    {
        [JsonPropertyName("summary")]
        public SummaryDto Summary { get; set; } = new();

        [JsonPropertyName("lecturerFeedbacks")]
        public List<LecturerFeedbackDto> LecturerFeedbacks { get; set; } = new();

        [JsonPropertyName("aiInsight")]
        public AiInsightDto AiInsight { get; set; } = new();

        [JsonPropertyName("aiSuggestion")]
        public AiSuggestionDto AiSuggestion { get; set; } = new();
    }

    public class SummaryDto
    {
        [JsonPropertyName("overallSummary")]
        public string OverallSummary { get; set; } = string.Empty;

        [JsonPropertyName("studentPerformance")]
        public string StudentPerformance { get; set; } = string.Empty;

        [JsonPropertyName("discussionFocus")]
        public string DiscussionFocus { get; set; } = string.Empty;
    }

    public class LecturerFeedbackDto
    {
        [JsonPropertyName("lecturer")]
        public string Lecturer { get; set; } = string.Empty;

        [JsonPropertyName("mainComments")]
        public string MainComments { get; set; } = string.Empty;

        [JsonPropertyName("positivePoints")]
        public List<string> PositivePoints { get; set; } = new();

        [JsonPropertyName("improvementPoints")]
        public List<string> ImprovementPoints { get; set; } = new();

        // ? Gi? nguyên nh?ng ??i double -> double? ?? tránh l?i khi AI tr? null
        [JsonPropertyName("rubricScores")]
        public Dictionary<string, double?> RubricScores { get; set; } = new();
    }

    public class AiInsightDto
    {
        [JsonPropertyName("analysis")]
        public string Analysis { get; set; } = string.Empty;

        [JsonPropertyName("rubricAverages")]
        public Dictionary<string, double?> RubricAverages { get; set; } = new();

        [JsonPropertyName("toneAnalysis")]
        public string ToneAnalysis { get; set; } = string.Empty;
    }

    public class AiSuggestionDto
    {
        [JsonPropertyName("forStudent")]
        public string ForStudent { get; set; } = string.Empty;

        [JsonPropertyName("forAdvisor")]
        public string ForAdvisor { get; set; } = string.Empty;

        [JsonPropertyName("forSystem")]
        public string ForSystem { get; set; } = string.Empty;
    }
}
