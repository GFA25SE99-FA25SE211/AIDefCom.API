using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class Lecturer : AppUser
    {
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Department { get; set; }
        public string? AcademicRank { get; set; } // e.g., Professor, Associate Professor, Lecturer
        public string? Degree { get; set; } // e.g., PhD, Master
    }
}
