using Microsoft.AspNetCore.Identity;
using System;

namespace AIDefCom.Repository.Entities
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public bool IsDelete { get; set; } = false;
        public bool HasGeneratedPassword { get; set; } = false;
        public string? LastGeneratedPassword { get; set; }
        public DateTime? PasswordGeneratedAt { get; set; }
    }
}
