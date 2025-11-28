using System.ComponentModel.DataAnnotations;

namespace AIDefCom.Service.Dto.Group
{
    public class GroupTotalScoreUpdateDto
    {
        [Required(ErrorMessage = "Total score is required")]
        [Range(0, 10, ErrorMessage = "Total score must be between 0 and 10")]
        public double TotalScore { get; set; }
    }
}
