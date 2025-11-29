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
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<StudentGroup> StudentGroups { get; set; }
        public DbSet<DefenseSession> DefenseSessions { get; set; }
        public DbSet<Transcript> Transcripts { get; set; }
        public DbSet<Rubric> Rubrics { get; set; }
        public DbSet<MajorRubric> MajorRubrics { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<CommitteeAssignment> CommitteeAssignments { get; set; }
        public DbSet<Council> Councils { get; set; }
        public DbSet<CouncilRole> CouncilRoles { get; set; }
        public DbSet<ProjectTask> Tasks { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<MemberNote> MemberNotes { get; set; }
        public DbSet<Recording> Recordings { get; set; }
        public DbSet<Note> Notes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // CONFIGURE TPT (Table-Per-Type) INHERITANCE
            // ============================================
            
            // AppUser table: AspNetUsers (already created by Identity)
            modelBuilder.Entity<AppUser>().ToTable("AspNetUsers");
            
            // Student table: Students (additional fields)
            // Students inherits from AppUser
            modelBuilder.Entity<Student>().ToTable("Students");
            
            // Lecturer table: Lecturers (additional fields)
            // Lecturers inherits from AppUser
            modelBuilder.Entity<Lecturer>().ToTable("Lecturers");

            // ============================================
            // STUDENT GROUP (Many-to-Many: Student <-> Group)
            // ============================================
            
            modelBuilder.Entity<StudentGroup>()
                .HasOne(sg => sg.Student)
                .WithMany()
                .HasForeignKey(sg => sg.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentGroup>()
                .HasOne(sg => sg.Group)
                .WithMany()
                .HasForeignKey(sg => sg.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: One student can be in a group only once
            modelBuilder.Entity<StudentGroup>()
                .HasIndex(sg => new { sg.UserId, sg.GroupId })
                .IsUnique();

            // ============================================
            // GROUP RELATIONSHIPS
            // ============================================
            
            modelBuilder.Entity<Group>()
                .HasOne(g => g.Semester)
                .WithMany()
                .HasForeignKey(g => g.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>()
                .HasOne(g => g.Major)
                .WithMany()
                .HasForeignKey(g => g.MajorId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // COUNCIL RELATIONSHIPS
            // ============================================
            
            modelBuilder.Entity<Council>()
                .HasOne(c => c.Major)
                .WithMany()
                .HasForeignKey(c => c.MajorId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // COMMITTEE ASSIGNMENT RELATIONSHIPS
            // ============================================
            
            modelBuilder.Entity<CommitteeAssignment>()
                .HasOne(ca => ca.Lecturer)
                .WithMany()
                .HasForeignKey(ca => ca.LecturerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommitteeAssignment>()
                .HasOne(ca => ca.Council)
                .WithMany()
                .HasForeignKey(ca => ca.CouncilId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CommitteeAssignment>()
                .HasOne(ca => ca.CouncilRole)
                .WithMany()
                .HasForeignKey(ca => ca.CouncilRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // DEFENSE SESSION RELATIONSHIPS
            // ============================================
            
            modelBuilder.Entity<DefenseSession>()
                .HasOne(d => d.Group)
                .WithMany()
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DefenseSession>()
                .HasOne(d => d.Council)
                .WithMany()
                .HasForeignKey(d => d.CouncilId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // PROJECT TASK RELATIONSHIPS
            // ============================================
            
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

            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.Rubric)
                .WithMany()
                .HasForeignKey(t => t.RubricId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.Session)
                .WithMany()
                .HasForeignKey(t => t.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: One Rubric per DefenseSession in ProjectTask
            modelBuilder.Entity<ProjectTask>()
                .HasIndex(t => new { t.SessionId, t.RubricId })
                .IsUnique();

            // ============================================
            // MEMBER NOTE RELATIONSHIPS
            // ============================================
            
            modelBuilder.Entity<MemberNote>()
                .HasOne(mn => mn.CommitteeAssignment)
                .WithMany()
                .HasForeignKey(mn => mn.CommitteeAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MemberNote>()
                .HasOne(mn => mn.Group)
                .WithMany()
                .HasForeignKey(mn => mn.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // TRANSCRIPT RELATIONSHIPS
            // ============================================
            
            modelBuilder.Entity<Transcript>()
                .HasOne(t => t.Session)
                .WithMany()
                .HasForeignKey(t => t.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // RECORDING RELATIONSHIPS
            // ============================================
            
            modelBuilder.Entity<Recording>()
                .HasOne(r => r.Transcript)
                .WithMany()
                .HasForeignKey(r => r.TranscriptId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Recording>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // REPORT RELATIONSHIPS
            // ============================================
            
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Session)
                .WithMany()
                .HasForeignKey(r => r.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // MAJOR RUBRIC RELATIONSHIPS
            // ============================================
            
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

            // ============================================
            // SCORE RELATIONSHIPS
            // ============================================
            
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

            // ============================================
            // NOTE RELATIONSHIPS
            // ============================================
            
            modelBuilder.Entity<Note>()
                .HasOne(n => n.Session)
                .WithOne()
                .HasForeignKey<Note>(n => n.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Note>()
                .HasIndex(n => n.SessionId)
                .IsUnique();
        }
    }
}