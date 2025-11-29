using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Rubric
{
    public class RubricCreateDto
    {
        public string RubricName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MajorId { get; set; }
    }
}
