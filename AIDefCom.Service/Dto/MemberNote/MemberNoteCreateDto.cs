using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Service.Dto.MemberNote
{
    public class MemberNoteCreateDto
    {
        [Required(ErrorMessage = "Lecturer ID is required")]
        public string LecturerId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Session ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Session ID must be greater than 0")]
        public int SessionId { get; set; }
        
        public string? NoteContent { get; set; }
    }
}
