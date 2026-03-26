using ChatSystem.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }

        IGenericRepository<FriendRequest> FriendRequests { get; }

        IGenericRepository<Friend> Friends { get; }

        IGenericRepository<Chat> Chats { get; }

        IGenericRepository<ChatUser> ChatUsers { get; }

        IGenericRepository<Message> Messages { get; }

        IGenericRepository<Notification> Notifications { get; }
        IGenericRepository<MessageRead> MessageReads { get; }
        IGenericRepository<Group> Groups { get; } 

        Task<int> SaveAsync();
    }
}
