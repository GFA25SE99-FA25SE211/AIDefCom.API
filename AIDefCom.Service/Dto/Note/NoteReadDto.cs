using System;

namespace AIDefCom.Service.Dto.Note
{
    public class NoteReadDto
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}