using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.DTOs.chating
{
    public class ChatSummaryDto
    {
        public int ChatId { get; set; }
        public ChatType Type { get; set; }

        // Private chat
        public string? FriendId { get; set; }
        public string? FriendName { get; set; }
        public string? FriendImage { get; set; }
        public UserStatus? FriendStatus { get; set; }

        // Group chat
        public string? GroupName { get; set; }
        public string? GroupImage { get; set; }

        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
