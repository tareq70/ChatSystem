using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.DTOs.chating
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string? SenderImage { get; set; }
        public string? Content { get; set; }
        public MessageType Type { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsOwn { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
    }
}
