using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.DefenseSession
{
    public class DefenseSessionTotalScoreUpdateDto
    {
        [Required(ErrorMessage = "Total score is required")]
        [Range(0, 10, ErrorMessage = "Total score must be between 0 and 10")]
        public double TotalScore { get; set; }
    }
}
