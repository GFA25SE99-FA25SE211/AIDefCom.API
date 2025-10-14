using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class Group
    {
        public string Id { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;
        public string TopicTitle_EN { get; set; } = string.Empty;
        public string TopicTitle_VN { get; set; } = string.Empty;
        public int SemesterId { get; set; } // Foreign Key
        public Semester? Semester { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}