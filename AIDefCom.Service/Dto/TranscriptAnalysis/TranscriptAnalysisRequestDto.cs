using System.ComponentModel.DataAnnotations;

namespace AIDefCom.Service.Dto.TranscriptAnalysis
{
    public class TranscriptAnalysisRequestDto
    {
        /// <summary>
        /// ID c?a Defense Session - dùng ?? l?y transcript t? Redis cache
        /// </summary>
        [Required(ErrorMessage = "Defense Session ID is required.")]
        public int DefenseSessionId { get; set; }
    }
}
