using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Rubric
{
    public class RubricUpdateDto
    {
        public string RubricName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
