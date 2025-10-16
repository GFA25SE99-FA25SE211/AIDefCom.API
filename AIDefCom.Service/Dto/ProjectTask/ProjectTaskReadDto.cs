using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.ProjectTask
{
    public class ProjectTaskReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string AssignedById { get; set; } = string.Empty;
        public string? AssignedByName { get; set; }
        public string AssignedToId { get; set; } = string.Empty;
        public string? AssignedToName { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
