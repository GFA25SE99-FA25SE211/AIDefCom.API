using System.ComponentModel.DataAnnotations;

namespace AIDefCom.Service.Dto.DefenseReport
{
    public class DefenseReportRequestDto
    {
        [Required(ErrorMessage = "Defense Session ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Defense Session ID must be greater than 0")]
        public int DefenseSessionId { get; set; }
    }
}
