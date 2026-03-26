using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Core.Entities
{
    public class Message
    {
        public int Id { get; set; }

        public int ChatId { get; set; }

        public string SenderId { get; set; }

        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public MessageType Type { get; set; } = MessageType.Text;
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public ICollection<MessageRead> Reads { get; set; } = new List<MessageRead>();
    }
}