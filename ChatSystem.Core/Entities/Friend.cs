using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Core.Entities
{
    public class Friend
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string FriendId { get; set; }

        public DateTime CreatedAt { get; set; }

        public FriendsStatus friendsStatus { get; set; } = FriendsStatus.Friend;
        public FriendsStatus UserStatus { get; set; } = FriendsStatus.Friend;

    }
}
