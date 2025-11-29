namespace AIDefCom.Service.Dto.Note
{
    public class NoteCreateDto
    {
        public int SessionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}