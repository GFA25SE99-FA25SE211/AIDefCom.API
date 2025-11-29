using System;

namespace AIDefCom.Repository.Entities
{
    public class Note
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public DefenseSession? Session { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}