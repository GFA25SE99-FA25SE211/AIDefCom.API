using System;
using System.Collections.Generic;

namespace AIDefCom.Service.Dto.AppUser
{
    public class AppUserResponseDto
    {
        public string Id { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool IsDelete { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();

        public bool HasGeneratedPassword { get; set; }
        public DateTime? PasswordGeneratedAt { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
