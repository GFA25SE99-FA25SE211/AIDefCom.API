using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.MemberNote
{
    public class MemberNoteReadDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string GroupId { get; set; } = string.Empty;
        public string? NoteContent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
