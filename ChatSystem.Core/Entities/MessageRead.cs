using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Core.Entities
{
    public class MessageRead
    {
        public int Id { get; set; }
        public int MessageId { get; set; }
        public string UserId { get; set; }
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    }
}
