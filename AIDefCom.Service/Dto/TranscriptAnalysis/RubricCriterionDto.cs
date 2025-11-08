namespace AIDefCom.Service.Dto.TranscriptAnalysis
{
    public class RubricCriterionDto
    {
        public string Name { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
