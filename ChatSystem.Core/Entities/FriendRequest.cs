using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Core.Entities
{
    public class FriendRequest
    {
        public int Id { get; set; }

        public string SenderId { get; set; }

        public string ReceiverId { get; set; }

        public FriendRequestStatus Status { get; set; } 

        public DateTime CreatedAt { get; set; }
    }
}
