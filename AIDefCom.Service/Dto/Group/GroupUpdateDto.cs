using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Group
{
    public class GroupUpdateDto
    {
        [Required(ErrorMessage = "Project code is required")]
        [StringLength(50, ErrorMessage = "Project code must not exceed 50 characters")]
        [RegularExpression(@"^[A-Z]{2,3}\d{2}[A-Z]{2}\d{2,3}$", 
            ErrorMessage = "Project code must follow format: FA25SE135 (2-3 letters, 2 digits, 2 letters, 2-3 digits)")]
        public string ProjectCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "English topic title is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "English topic title must be between 10 and 500 characters")]
        public string TopicTitle_EN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vietnamese topic title is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Vietnamese topic title must be between 10 and 500 characters")]
        public string TopicTitle_VN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Semester ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Semester ID must be greater than 0")]
        public int SemesterId { get; set; }

        [Required(ErrorMessage = "Major ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Major ID must be greater than 0")]
        public int MajorId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [RegularExpression(@"^(Active|Inactive|Completed|Pending|Cancelled)$", 
            ErrorMessage = "Status must be one of: Active, Inactive, Completed, Pending, Cancelled")]
        public string Status { get; set; } = string.Empty;
    }
}
