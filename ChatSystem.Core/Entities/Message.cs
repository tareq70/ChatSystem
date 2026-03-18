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

        public string Content { get; set; }

        public string MessageType { get; set; } 

        public DateTime CreatedAt { get; set; }

        public bool IsSeen { get; set; }
    }
}