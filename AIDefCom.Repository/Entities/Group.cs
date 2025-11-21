using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class Group
    {
        public string Id { get; set; } = string.Empty; // Primary Key
        public string TopicTitle_EN { get; set; } = string.Empty;
        public string TopicTitle_VN { get; set; } = string.Empty;
        
        // Foreign Key to Semester
        public int SemesterId { get; set; }
        public Semester? Semester { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        // Foreign Key to Major
        public int MajorId { get; set; }
        public Major? Major { get; set; }
        
        public string ProjectCode { get; set; } = string.Empty;
        
        // Soft Delete
        public bool IsDeleted { get; set; } = false;
    }
}