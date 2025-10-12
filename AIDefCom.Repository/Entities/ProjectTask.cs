using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class ProjectTask
    {
        public int Id { get; set; } // Primary Key
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Foreign Key to AppUser (Assigned_by)
        public string AssignedById { get; set; } = string.Empty;
        public AppUser? AssignedBy { get; set; }

        // Foreign Key to AppUser (Assigned_to)
        public string AssignedToId { get; set; } = string.Empty;
        public AppUser? AssignedTo { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}