using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.DefenseSession
{
    public class DefenseSessionCreateDto
    {
        [Required(ErrorMessage = "Group ID is required")]
        [StringLength(450, ErrorMessage = "Group ID cannot exceed 450 characters")]
        public string GroupId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Location must be between 5 and 500 characters")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Defense date is required")]
        public DateTime DefenseDate { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Council ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Council ID must be greater than 0")]
        public int CouncilId { get; set; }
    }
}
