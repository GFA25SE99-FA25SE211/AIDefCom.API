using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class Semester
    {
        public int Id { get; set; } // Primary Key
        public string SemesterName { get; set; } = string.Empty;
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Soft Delete
        public bool IsDeleted { get; set; } = false;
    }
}