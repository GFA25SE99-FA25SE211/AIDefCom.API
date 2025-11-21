using System;
using System.ComponentModel.DataAnnotations;

namespace AIDefCom.Service.Dto.Account
{
    public class UpdateAccountDto
    {
        [Required]
        public string FullName { get; set; } = default!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        public string? PhoneNumber { get; set; }

        // Optional: Admin can reset password without knowing current password
        public string? NewPassword { get; set; }
        
        public string? ConfirmNewPassword { get; set; }
    }
}
