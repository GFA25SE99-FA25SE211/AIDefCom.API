using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.CommitteeAssignment
{
    public class CommitteeAssignmentUpdateDto
    {
        [Required(ErrorMessage = "Lecturer ID is required")]
        [StringLength(450, ErrorMessage = "Lecturer ID cannot exceed 450 characters")]
        public string LecturerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Council ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Council ID must be greater than 0")]
        public int CouncilId { get; set; }

        [Required(ErrorMessage = "Council Role ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Council Role ID must be greater than 0")]
        public int CouncilRoleId { get; set; }
    }
}
