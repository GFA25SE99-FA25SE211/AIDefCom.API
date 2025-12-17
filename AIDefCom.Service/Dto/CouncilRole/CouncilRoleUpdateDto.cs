using System.ComponentModel.DataAnnotations;

namespace AIDefCom.Service.Dto.CouncilRole
{
    public class CouncilRoleUpdateDto
    {
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(100, ErrorMessage = "Role name cannot exceed 100 characters")]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }
}
