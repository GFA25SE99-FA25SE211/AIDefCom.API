namespace AIDefCom.Service.Dto.AppUser
{
    public class AppUserListDto
    {
        public string Id { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Role { get; set; }

        public bool IsDelete { get; set; }

        public bool EmailConfirmed { get; set; }
    }
}
