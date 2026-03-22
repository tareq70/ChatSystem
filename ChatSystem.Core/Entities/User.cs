using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace ChatSystem.Core.Entities
{
    public class User : IdentityUser
    {
        public string? ProfileImage { get; set; }

        public UserStatus Status { get; set; } = UserStatus.Offline;
        public DateTime? LastSeen { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? OtpCode { get; set; }
        public DateTime? OtpExpiry { get; set; }
    }
}
