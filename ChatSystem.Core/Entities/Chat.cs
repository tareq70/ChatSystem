using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Core.Entities
{
    public class Chat
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public ChatType Type { get; set; } 

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
