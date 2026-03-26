using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Core.Entities
{
    public class Group
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string Name { get; set; }
        public string? GroupImage { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
