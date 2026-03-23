using ChatSystem.Application.Interfaces;
using ChatSystem.Core.Entities;
using ChatSystem.Infrastructure.Data;
using ChatSystem.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Users = new GenericRepository<User>(_context);
            FriendRequests = new GenericRepository<FriendRequest>(_context);
            Friends = new GenericRepository<Friend>(_context);
            Chats = new GenericRepository<Chat>(_context);
            ChatUsers = new GenericRepository<ChatUser>(_context);
            Messages = new GenericRepository<Message>(_context);
            Notifications = new GenericRepository<Notification>(_context);
        }
      
        public IGenericRepository<User> Users { get; private set; }

        public IGenericRepository<FriendRequest> FriendRequests { get; private set; }

        public IGenericRepository<Friend> Friends { get; private set; }

        public IGenericRepository<Chat> Chats { get; private set; }

        public IGenericRepository<ChatUser> ChatUsers { get; private set; }

        public IGenericRepository<Message> Messages { get; private set; }

        public IGenericRepository<Notification> Notifications { get; private set; }


        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
