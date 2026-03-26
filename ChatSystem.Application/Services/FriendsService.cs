using ChatSystem.Application.DTOs.Friends;
using ChatSystem.Application.Interfaces;
using ChatSystem.Core.Entities;
using ChatSystem.Core.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Services
{
    public class FriendsService : IFriendsService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<User> _userManager;

        public FriendsService(IUnitOfWork uow, UserManager<User> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        public async Task<List<FriendRequestDto>> GetPendingRequestsAsync(string userId)
        {
            var requests = await _uow.FriendRequests.FindAsync(r =>
                r.ReceiverId == userId &&
                r.Status == FriendRequestStatus.Pending);

            var result = new List<FriendRequestDto>();

            foreach (var request in requests)
            {
                var sender = await _userManager.FindByIdAsync(request.SenderId);
                if (sender is null) continue;

                result.Add(new FriendRequestDto
                {
                    RequestId = request.Id,
                    SenderId = sender.Id,
                    SenderName = sender.UserName,
                    SenderImage = sender.ProfileImage,
                    SenderStatus = sender.Status,
                    CreatedAt = request.CreatedAt,

                });
            }

            return result;
        }

        public async Task<List<FriendsDto>> GetMyFriendsAsync(string userId)
        {
            var friends = await _uow.Friends.FindAsync(f =>
                f.UserId == userId || f.FriendId == userId);

            var result = new List<FriendsDto>();

            foreach (var friend in friends)
            {
                var friendId = friend.UserId == userId ? friend.FriendId : friend.UserId;
                var user = await _userManager.FindByIdAsync(friendId);
                if (user is null) continue;

                // أنا UserId أو FriendId؟
                var myStatus = friend.UserId == userId
                    ? friend.UserStatus
                    : friend.friendsStatus;

                var theirStatus = friend.UserId == userId
                    ? friend.friendsStatus
                    : friend.UserStatus;

                result.Add(new FriendsDto
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    ProfileImage = user.ProfileImage,
                    Status = user.Status,
                    LastSeen = user.LastSeen,
                    MyStatus = myStatus,
                    TheirStatus = theirStatus
                });
            }

            return result;
        }
        public async Task RemoveFriendAsync(string userId, string friendId)
        {
            var friends = await _uow.Friends.FindAsync(f =>
                (f.UserId == userId && f.FriendId == friendId) ||
                (f.UserId == friendId && f.FriendId == userId));

            var friend = friends.FirstOrDefault();
            if (friend is null) return;

            _uow.Friends.Delete(friend);

            var requests = await _uow.FriendRequests.FindAsync(r =>
                (r.SenderId == userId && r.ReceiverId == friendId) ||
                (r.SenderId == friendId && r.ReceiverId == userId));

            foreach (var request in requests)
                _uow.FriendRequests.Delete(request);

            await _uow.SaveAsync();
        }


        public async Task BlockUserAsync(string blockerId, string blockedId)
        {
            var friends = await _uow.Friends.FindAsync(f =>
                (f.UserId == blockerId && f.FriendId == blockedId) ||
                (f.UserId == blockedId && f.FriendId == blockerId));

            var friend = friends.FirstOrDefault();
            if (friend is null) return;

            if (friend.UserId == blockerId)
                friend.UserStatus = FriendsStatus.Blocked;
            else
                friend.friendsStatus = FriendsStatus.Blocked;

            _uow.Friends.Update(friend);
            await _uow.SaveAsync();
        }

        public async Task UnblockUserAsync(string blockerId, string blockedId)
        {
            var friends = await _uow.Friends.FindAsync(f =>
                (f.UserId == blockerId && f.FriendId == blockedId) ||
                (f.UserId == blockedId && f.FriendId == blockerId));

            var friend = friends.FirstOrDefault();
            if (friend is null) return;

            if (friend.UserId == blockerId)
                friend.UserStatus = FriendsStatus.Friend;
            else if (friend.FriendId == blockerId)
                friend.friendsStatus = FriendsStatus.Friend;
            else
                return; 

            _uow.Friends.Update(friend);
            await _uow.SaveAsync();
        }

        public async Task<bool> IsBlockedAsync(string userId, string targetId)
        {
            var friends = await _uow.Friends.FindAsync(f =>
                (f.UserId == userId && f.FriendId == targetId) ||
                (f.UserId == targetId && f.FriendId == userId));

            var friend = friends.FirstOrDefault();
            if (friend is null) return false;

            if (friend.UserId == userId)
                return friend.UserStatus == FriendsStatus.Blocked ||
                       friend.friendsStatus == FriendsStatus.Blocked;
            else
                return friend.friendsStatus == FriendsStatus.Blocked ||
                       friend.UserStatus == FriendsStatus.Blocked;
        }

        public async Task<bool> IsBlockerAsync(string userId, string targetId)
        {
            var friends = await _uow.Friends.FindAsync(f =>
                (f.UserId == userId && f.FriendId == targetId) ||
                (f.UserId == targetId && f.FriendId == userId));

            var friend = friends.FirstOrDefault();
            if (friend is null) return false;

            if (friend.UserId == userId)
                return friend.UserStatus == FriendsStatus.Blocked;
            else
                return friend.friendsStatus == FriendsStatus.Blocked;
        }

    }
}