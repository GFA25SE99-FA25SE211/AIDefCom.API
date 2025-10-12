using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class MajorRubric
    {
        public int Id { get; set; } // Primary Key

        // Foreign Key to Major
        public int MajorId { get; set; }
        public Major? Major { get; set; }

        // Foreign Key to Rubric
        public int RubricId { get; set; }
        public Rubric? Rubric { get; set; }
    }
}