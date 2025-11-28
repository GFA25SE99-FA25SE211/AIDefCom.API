using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Council
{
    public class CouncilCreateDto
    {
        [Required(ErrorMessage = "Major ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Major ID must be greater than 0")]
        public int MajorId { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
