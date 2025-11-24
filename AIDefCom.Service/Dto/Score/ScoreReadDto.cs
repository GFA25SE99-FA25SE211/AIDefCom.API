using System;

namespace AIDefCom.Service.Dto.Score
{
    public class ScoreReadDto
    {
        public int Id { get; set; }
        public double Value { get; set; }
        
        // Rubric info
        public int RubricId { get; set; }
        public string? RubricName { get; set; }
        
        // Evaluator info
        public string EvaluatorId { get; set; } = string.Empty;
        public string? EvaluatorName { get; set; }
        
        // Student info
        public string StudentId { get; set; } = string.Empty;
        public string? StudentName { get; set; }
        
        // Session info
        public int SessionId { get; set; }
        
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
