using System.Collections.Generic;

namespace AIDefCom.Service.Dto.TranscriptAnalysis
{
    public class LecturerEvaluationDto
    {
        public string Lecturer { get; set; } = string.Empty;
        public List<RubricCriterionDto> Criteria { get; set; } = new();
    }
}
