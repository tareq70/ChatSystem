using ChatSystem.Application.Interfaces;
using ChatSystem.Core.Entities;
using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Services
{
    public class FriendRequestService : IFriendRequestService
    {
        private readonly IUnitOfWork _uow;

        public FriendRequestService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // 1. send request
        public async Task SendRequestAsync(string senderId, string receiverId)
        {
            // check if there's already a pending or accepted request between these users
            var existing = await _uow.FriendRequests.FindAsync(r =>
                (r.SenderId == senderId && r.ReceiverId == receiverId) ||
                (r.SenderId == receiverId && r.ReceiverId == senderId));

            if (existing.Any()) return;

            await _uow.FriendRequests.AddAsync(new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = FriendRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });

            await _uow.SaveAsync();
        }

        // 2. Accept request
        public async Task AcceptRequestAsync(int requestId, string currentUserId)
        {
            var request = await _uow.FriendRequests.GetByIdAsync(requestId);

            if (request is null || request.ReceiverId != currentUserId) return;

            // Update request status
            request.Status = FriendRequestStatus.Accepted;
            _uow.FriendRequests.Update(request);

            // Add to friends for both users
            await _uow.Friends.AddAsync(new Friend
            {
                UserId = request.SenderId,
                FriendId = request.ReceiverId,
                CreatedAt = DateTime.UtcNow
            });

            await _uow.SaveAsync();
        }

        // 3. Reject request 
        public async Task RejectRequestAsync(int requestId, string currentUserId)
        {
            var request = await _uow.FriendRequests.GetByIdAsync(requestId);

            if (request is null || request.ReceiverId != currentUserId) return;

            request.Status = FriendRequestStatus.Rejected;
            _uow.FriendRequests.Update(request);

            await _uow.SaveAsync();
        }

        // 4. Cancel request 
        public async Task CancelRequestAsync(int requestId, string currentUserId)
        {
            var request = await _uow.FriendRequests.GetByIdAsync(requestId);

            if (request is null || request.SenderId != currentUserId) return;

            _uow.FriendRequests.Delete(request);

            await _uow.SaveAsync();
        }

    }
}