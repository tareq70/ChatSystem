using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.DTOs.Friends
{
    public class FriendsDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string? ProfileImage { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastSeen { get; set; }
        public FriendsStatus MyStatus { get; set; }      
        public FriendsStatus TheirStatus { get; set; }   
        public string? BlockerId { get; set; }
    }
}
