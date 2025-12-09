using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Major
{
    public class MajorCreateDto
    {
        [Required(ErrorMessage = "Major name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Major name must be between 2 and 200 characters")]
        [RegularExpression(@"^[\p{L}\p{N}\s\-_.,()&]+$", ErrorMessage = "Major name contains invalid characters")]
        public string MajorName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
    }
}
