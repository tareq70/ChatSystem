using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.DTOs.Friends
{
    public class FriendRequestDto
    {
        public int RequestId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string? SenderImage { get; set; }
        public UserStatus SenderStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
