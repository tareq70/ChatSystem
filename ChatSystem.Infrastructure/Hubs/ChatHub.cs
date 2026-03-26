using ChatSystem.Application.DTOs.chating;
using ChatSystem.Application.Interfaces;
using ChatSystem.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Infrastructure.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {

        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier!;
            var chatList = await _chatService.GetChatListAsync(userId);
            foreach (var chat in chatList)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chat.ChatId}");

            await base.OnConnectedAsync();
        }
        public async Task JoinChat(int chatId)
        {
            var userId = Context.UserIdentifier!;
            if (!await _chatService.CanAccessChatAsync(userId, chatId)) return;

            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");

            await _chatService.MarkMessagesAsReadAsync(chatId, userId);
            await Clients.Group($"chat_{chatId}")
                .SendAsync("MessagesRead", chatId, userId);
        }

        public async Task SendMessage(int chatId, string? content, string type = "Text",
                                      string? fileUrl = null, string? fileName = null)
        {
            var userId = Context.UserIdentifier!;

            var msgType = Enum.Parse<MessageType>(type);
            var dto = new SendMessageDto
            {
                ChatId = chatId,
                Content = content,
                Type = msgType,
                FileUrl = fileUrl,
                FileName = fileName
            };

            var message = await _chatService.SendMessageAsync(userId, dto);

            await Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", message);
        }

        public async Task Typing(int chatId, bool isTyping)
        {
            var userId = Context.UserIdentifier!;
            await Clients.OthersInGroup($"chat_{chatId}")
                .SendAsync("TypingIndicator", userId, isTyping);
        }

        public async Task MarkRead(int chatId)
        {
            

            var userId = Context.UserIdentifier!;
            await _chatService.MarkMessagesAsReadAsync(chatId, userId);

            
            await Clients.Group($"chat_{chatId}")
                .SendAsync("MessagesRead", chatId, userId);
        }
    }
}