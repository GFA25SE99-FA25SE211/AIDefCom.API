using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.ProjectTask
{
    public class ProjectTaskCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string AssignedById { get; set; } = string.Empty;
        public string AssignedToId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
