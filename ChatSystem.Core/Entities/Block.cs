using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Core.Entities
{
    public class Block
    {
        public int Id { get; set; }
        public string BlockerId { get; set; }
        public string BlockedId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
