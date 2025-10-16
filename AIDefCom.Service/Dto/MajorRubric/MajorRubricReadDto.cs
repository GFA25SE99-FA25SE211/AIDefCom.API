using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.MajorRubric
{
    public class MajorRubricReadDto
    {
        public int Id { get; set; }
        public int MajorId { get; set; }
        public string? MajorName { get; set; }
        public int RubricId { get; set; }
        public string? RubricName { get; set; }
    }
}
