using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.DTOs.User_Profile
{
    public class UserProfileDTO
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
        public UserStatus Status { get; set; }
        public DateTime LastSeen { get; set; }
        public DateTime CreatedAt { get; set; }
        public int FriendsCount { get; set; }
    }
}
