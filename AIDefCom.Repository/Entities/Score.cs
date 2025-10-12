using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class Score
    {
        public int Id { get; set; } // Primary Key
        public double Value { get; set; }

        // Foreign Key to Rubric
        public int RubricId { get; set; }
        public Rubric? Rubric { get; set; }

        // Foreign Key to AppUser (Evaluator)
        public string EvaluatorId { get; set; } = string.Empty;
        public AppUser? Evaluator { get; set; }

        // Foreign Key to Student
        public string StudentId { get; set; } = string.Empty;
        public Student? Student { get; set; }

        // Foreign Key to DefenseSession
        public int SessionId { get; set; }
        public DefenseSession? Session { get; set; }

        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}