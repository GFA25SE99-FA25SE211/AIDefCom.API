using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.Semester
{
    public class SemesterReadDto
    {
        public int Id { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MajorId { get; set; }
        public string? MajorName { get; set; }
    }
}
