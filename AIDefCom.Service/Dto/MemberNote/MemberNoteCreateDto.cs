using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.MemberNote
{
    public class MemberNoteCreateDto
    {
        public string UserId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string? NoteContent { get; set; }
    }
}
