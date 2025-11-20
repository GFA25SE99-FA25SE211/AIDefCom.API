using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class Major
    {
        public int Id { get; set; } // Primary Key
        public string MajorName { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Soft Delete
        public bool IsDeleted { get; set; } = false;
    }
}