using ChatSystem.Application.DTOs.chating;
using ChatSystem.Application.Interfaces;
using ChatSystem.Core.Entities;
using ChatSystem.Core.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<User> _userManager;

        public ChatService(IUnitOfWork uow, UserManager<User> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        public async Task<int> GetOrCreatePrivateChatAsync(string currentUserId, string friendId)
        {
            var allFriends = await _uow.Friends.GetAllAsync(); 
            foreach (var f in allFriends)
            {
                Console.WriteLine($"{f.UserId} - {f.FriendId} - {f.UserStatus}");
            }
            var friendship = (await _uow.Friends.FindAsync(f =>
                (f.UserId == currentUserId && f.FriendId == friendId) ||
                (f.UserId == friendId && f.FriendId == currentUserId)
            )).FirstOrDefault(); 

            if (friendship is null) throw new UnauthorizedAccessException("Not friends.");

            bool blocked =
                (friendship.UserId == currentUserId && friendship.UserStatus == FriendsStatus.Blocked) ||
                (friendship.FriendId == currentUserId && friendship.friendsStatus == FriendsStatus.Blocked);

            if (blocked) throw new UnauthorizedAccessException("Blocked.");

            var myChats = (await _uow.ChatUsers.FindAsync(cu => cu.UserId == currentUserId)).Select(cu => cu.ChatId).ToHashSet();
            var theirChats = (await _uow.ChatUsers.FindAsync(cu => cu.UserId == friendId)).Select(cu => cu.ChatId).ToHashSet();
            var shared = myChats.Intersect(theirChats).ToList();

            foreach (var cid in shared)
            {
                var existing = await _uow.Chats.GetByIdAsync(cid);
                if (existing?.Type == ChatType.Private) return cid;
            }

            var chat = new Chat { Type = ChatType.Private };
            await _uow.Chats.AddAsync(chat);
            await _uow.SaveAsync();

            await _uow.ChatUsers.AddAsync(new ChatUser { ChatId = chat.Id, UserId = currentUserId });
            await _uow.ChatUsers.AddAsync(new ChatUser { ChatId = chat.Id, UserId = friendId });
            await _uow.SaveAsync();

            return chat.Id;
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(int chatId, string currentUserId)
        {
            if (!await CanAccessChatAsync(currentUserId, chatId))
                throw new UnauthorizedAccessException();

            var messages = (await _uow.Messages.FindAsync(m => m.ChatId == chatId && !m.IsDeleted))
                .OrderBy(m => m.SentAt);

            var result = new List<MessageDto>();
            foreach (var m in messages)
            {
                var sender = await _userManager.FindByIdAsync(m.SenderId);
                var reads = await _uow.MessageReads.FindAsync(r => r.MessageId == m.Id);
                result.Add(new MessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = sender?.UserName ?? "Unknown",
                    SenderImage = sender?.ProfileImage,
                    Content = m.Content,
                    Type = m.Type,
                    FileUrl = m.FileUrl,
                    FileName = m.FileName,
                    SentAt = m.SentAt,
                    IsOwn = m.SenderId == currentUserId,
                    IsRead = reads.Any(r => r.UserId != m.SenderId),
                    IsDeleted = m.IsDeleted
                });
            }
            return result;
        }

        public async Task<MessageDto> SendMessageAsync(string senderId, SendMessageDto dto)
        {
            if (!await CanAccessChatAsync(senderId, dto.ChatId))
                throw new UnauthorizedAccessException();

            var message = new Message
            {
                ChatId = dto.ChatId,
                SenderId = senderId,
                Content = dto.Content?.Trim(),
                Type = dto.Type,
                FileUrl = dto.FileUrl,
                FileName = dto.FileName,
                SentAt = DateTime.UtcNow
            };

            await _uow.Messages.AddAsync(message);
            await _uow.SaveAsync();

            var sender = await _userManager.FindByIdAsync(senderId);
            return new MessageDto
            {
                Id = message.Id,
                SenderId = senderId,
                SenderName = sender?.UserName ?? "Unknown",
                SenderImage = sender?.ProfileImage,
                Content = message.Content,
                Type = message.Type,
                FileUrl = message.FileUrl,
                FileName = message.FileName,
                SentAt = message.SentAt,
                IsOwn = true,
                IsRead = false
            };
        }

        public async Task<IEnumerable<ChatSummaryDto>> GetChatListAsync(string userId)
        {
            var userChatIds = (await _uow.ChatUsers.FindAsync(cu => cu.UserId == userId))
                .Select(cu => cu.ChatId).ToList();

            var result = new List<ChatSummaryDto>();

            foreach (var chatId in userChatIds)
            {
                var chat = await _uow.Chats.GetByIdAsync(chatId);
                if (chat is null) continue;

                var lastMsg = (await _uow.Messages.FindAsync(m => m.ChatId == chatId && !m.IsDeleted))
                    .OrderByDescending(m => m.SentAt).FirstOrDefault();
                var unread = await GetUnreadCountAsync(chatId, userId);
                var summary = new ChatSummaryDto
                {
                    ChatId = chatId,
                    Type = chat.Type,
                    LastMessage = lastMsg?.Type == MessageType.Text ? lastMsg.Content : $"📎 {lastMsg?.FileName}",
                    LastMessageAt = lastMsg?.SentAt,
                    UnreadCount = unread
                };

                if (chat.Type == ChatType.Private)
                {
                    var friendCu = (await _uow.ChatUsers.FindAsync(cu => cu.ChatId == chatId && cu.UserId != userId)).FirstOrDefault();
                    var friend = friendCu is not null ? await _userManager.FindByIdAsync(friendCu.UserId) : null;
                    summary.FriendId = friend?.Id;
                    summary.FriendName = friend?.UserName;
                    summary.FriendImage = friend?.ProfileImage;
                    summary.FriendStatus = friend?.Status;
                }
                else
                {
                    var group = (await _uow.Groups.FindAsync(g => g.ChatId == chatId)).FirstOrDefault();
                    summary.GroupName = group?.Name;
                    summary.GroupImage = group?.GroupImage;
                }

                result.Add(summary);
            }

            return result.OrderByDescending(c => c.LastMessageAt);
        }

        public async Task<bool> CanAccessChatAsync(string userId, int chatId)
        {
            var members = await _uow.ChatUsers.FindAsync(cu => cu.ChatId == chatId);
            return members.Any(cu => cu.UserId == userId);
        }

        public async Task MarkMessagesAsReadAsync(int chatId, string userId)
        {
            var messages = (await _uow.Messages.FindAsync(m => m.ChatId == chatId && m.SenderId != userId)).ToList();
            foreach (var msg in messages)
            {
                var alreadyRead = (await _uow.MessageReads.FindAsync(r => r.MessageId == msg.Id && r.UserId == userId)).Any();
                if (!alreadyRead)
                    await _uow.MessageReads.AddAsync(new MessageRead { MessageId = msg.Id, UserId = userId });
            }
            await _uow.SaveAsync();
        }

        public async Task<int> GetUnreadCountAsync(int chatId, string userId)
        {
            var messages = (await _uow.Messages.FindAsync(m => m.ChatId == chatId && m.SenderId != userId)).ToList();
            int unread = 0;
            foreach (var msg in messages)
            {
                var read = (await _uow.MessageReads.FindAsync(r => r.MessageId == msg.Id && r.UserId == userId)).Any();
                if (!read) unread++;
            }
            return unread;
        }
    }
}