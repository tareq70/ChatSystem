using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Interfaces
{
    public interface IFriendRequestService
    {
        Task SendRequestAsync(string senderId, string receiverId);
        Task AcceptRequestAsync(int requestId, string currentUserId);
        Task RejectRequestAsync(int requestId, string currentUserId);
        Task CancelRequestAsync(int requestId, string currentUserId);
    }
}
