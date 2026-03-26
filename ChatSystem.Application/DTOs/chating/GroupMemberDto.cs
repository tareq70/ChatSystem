using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.DTOs.chating
{
    public class GroupMemberDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string? ProfileImage { get; set; }
        public UserStatus Status { get; set; }
        public bool IsAdmin { get; set; }
    }
}
