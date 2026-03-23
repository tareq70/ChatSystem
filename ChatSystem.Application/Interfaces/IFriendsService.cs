using ChatSystem.Application.DTOs.Friends;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Interfaces
{
    public interface IFriendsService
    {
        Task<List<FriendRequestDto>> GetPendingRequestsAsync(string userId);
        Task<List<FriendsDto>> GetMyFriendsAsync(string userId);

        Task RemoveFriendAsync(string userId, string friendId);
        Task BlockUserAsync(string blockerId, string blockedId);
        Task UnblockUserAsync(string blockerId, string blockedId);
        Task<bool> IsBlockedAsync(string userId, string targetId);
        Task<bool> IsBlockerAsync(string userId, string targetId);


    }
}
