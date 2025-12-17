using System;

namespace AIDefCom.Service.Dto.CouncilRole
{
    public class CouncilRoleReadDto
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
