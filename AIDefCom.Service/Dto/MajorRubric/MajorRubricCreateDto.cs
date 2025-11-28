using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.MajorRubric
{
    public class MajorRubricCreateDto
    {
        [Required(ErrorMessage = "Major ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Major ID must be greater than 0")]
        public int MajorId { get; set; }

        [Required(ErrorMessage = "Rubric ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Rubric ID must be greater than 0")]
        public int RubricId { get; set; }
    }
}
