using AIDefCom.Repository.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet declarations for all entities
        public DbSet<Major> Majors { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<DefenseSession> DefenseSessions { get; set; }
        public DbSet<Transcript> Transcripts { get; set; }
        public DbSet<Rubric> Rubrics { get; set; }
        public DbSet<MajorRubric> MajorRubrics { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<CommitteeAssignment> CommitteeAssignments { get; set; }
        public DbSet<Council> Councils { get; set; }
        public DbSet<ProjectTask> Tasks { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<MemberNote> MemberNotes { get; set; }
        public DbSet<Recording> Recordings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Student: UserId is string, GroupId is int
            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Group)
                .WithMany()
                .HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Semester: MajorId is int
            modelBuilder.Entity<Semester>()
                .HasOne(s => s.Major)
                .WithMany()
                .HasForeignKey(s => s.MajorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Group: SemesterId is int
            modelBuilder.Entity<Group>()
                .HasOne(g => g.Semester)
                .WithMany()
                .HasForeignKey(g => g.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            // DefenseSession: GroupId is int
            modelBuilder.Entity<DefenseSession>()
                .HasOne(d => d.Group)
                .WithMany()
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transcript: SessionId is int
            modelBuilder.Entity<Transcript>()
                .HasOne(t => t.Session)
                .WithMany()
                .HasForeignKey(t => t.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Recording: TranscriptId is optional int FK
            modelBuilder.Entity<Recording>()
                .HasOne(r => r.Transcript)
                .WithMany()
                .HasForeignKey(r => r.TranscriptId)
                .OnDelete(DeleteBehavior.SetNull);

            // Recording: UserId is string FK to AspNetUsers
            modelBuilder.Entity<Recording>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Report: SessionId is int
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Session)
                .WithMany()
                .HasForeignKey(r => r.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // MajorRubric: MajorId, RubricId
            modelBuilder.Entity<MajorRubric>()
                .HasOne(mr => mr.Major)
                .WithMany()
                .HasForeignKey(mr => mr.MajorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MajorRubric>()
                .HasOne(mr => mr.Rubric)
                .WithMany()
                .HasForeignKey(mr => mr.RubricId)
                .OnDelete(DeleteBehavior.Restrict);

            // CommitteeAssignment: UserId, CouncilId, SessionId
            modelBuilder.Entity<CommitteeAssignment>()
                .HasOne(ca => ca.User)
                .WithMany()
                .HasForeignKey(ca => ca.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommitteeAssignment>()
                .HasOne(ca => ca.Council)
                .WithMany()
                .HasForeignKey(ca => ca.CouncilId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommitteeAssignment>()
                .HasOne(ca => ca.Session)
                .WithMany()
                .HasForeignKey(ca => ca.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProjectTask: AssignedById, AssignedToId
            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.AssignedBy)
                .WithMany()
                .HasForeignKey(t => t.AssignedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.AssignedTo)
                .WithMany()
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            // Score: RubricId, EvaluatorId, StudentId, SessionId
            modelBuilder.Entity<Score>()
                .HasOne(s => s.Rubric)
                .WithMany()
                .HasForeignKey(s => s.RubricId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Score>()
                .HasOne(s => s.Evaluator)
                .WithMany()
                .HasForeignKey(s => s.EvaluatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Score>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Score>()
                .HasOne(s => s.Session)
                .WithMany()
                .HasForeignKey(s => s.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // MemberNote: UserId, GroupId
            modelBuilder.Entity<MemberNote>()
                .HasOne(mn => mn.User)
                .WithMany()
                .HasForeignKey(mn => mn.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MemberNote>()
                .HasOne(mn => mn.Group)
                .WithMany()
                .HasForeignKey(mn => mn.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
