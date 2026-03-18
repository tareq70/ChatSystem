using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Core.Entities
{
    public class ChatUser
    {
        public int Id { get; set; }

        public int ChatId { get; set; }

        public string UserId { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}
