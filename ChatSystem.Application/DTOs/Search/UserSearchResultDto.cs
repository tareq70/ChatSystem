using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.DTOs.Search
{
    public class UserSearchResultDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? ProfileImage { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastSeen { get; set; }  

        public FriendRequestStatus? RequestStatus { get; set; }
    }
}
