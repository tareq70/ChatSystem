using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.DTOs.Friends
{
    public class FriendsPageDto
    {
        public List<FriendRequestDto> PendingRequests { get; set; } = new();
        public List<FriendsDto> MyFriends { get; set; } = new();
    }
}
