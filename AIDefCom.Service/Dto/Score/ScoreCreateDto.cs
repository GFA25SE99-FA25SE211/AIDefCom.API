namespace AIDefCom.Service.Dto.Score
{
    public class ScoreCreateDto
    {
        public double Value { get; set; }
        public int RubricId { get; set; }
        public string EvaluatorId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public int SessionId { get; set; }
        public string? Comment { get; set; }
    }
}
