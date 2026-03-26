using ChatSystem.Application.DTOs.chating;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Interfaces
{
    public interface IChatService
    {
        Task<int> GetOrCreatePrivateChatAsync(string currentUserId, string friendId);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(int chatId, string currentUserId);
        Task<MessageDto> SendMessageAsync(string senderId, SendMessageDto dto);
        Task<IEnumerable<ChatSummaryDto>> GetChatListAsync(string userId);
        Task<bool> CanAccessChatAsync(string userId, int chatId);
        Task MarkMessagesAsReadAsync(int chatId, string userId);
        Task<int> GetUnreadCountAsync(int chatId, string userId);
    }
}
