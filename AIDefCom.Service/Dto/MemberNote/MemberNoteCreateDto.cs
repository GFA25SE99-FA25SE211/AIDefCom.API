using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.MemberNote
{
    public class MemberNoteCreateDto
    {
        public string CommitteeAssignmentId { get; set; } = string.Empty;
        public int SessionId { get; set; }
        public string? NoteContent { get; set; }
    }
}
