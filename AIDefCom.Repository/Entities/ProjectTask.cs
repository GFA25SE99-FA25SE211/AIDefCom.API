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

        // Foreign Key to CommitteeAssignment (Assigned_by)
        public string AssignedById { get; set; } = string.Empty;
        public CommitteeAssignment? AssignedBy { get; set; }

        // Foreign Key to CommitteeAssignment (Assigned_to)
        public string AssignedToId { get; set; } = string.Empty;
        public CommitteeAssignment? AssignedTo { get; set; }

        // Foreign Key to Rubric
        public int RubricId { get; set; }
        public Rubric? Rubric { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}