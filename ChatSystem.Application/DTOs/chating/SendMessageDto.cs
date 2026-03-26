using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.DTOs.chating
{
    public class SendMessageDto
    {
        public int ChatId { get; set; }
        public string? Content { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
    }
}
