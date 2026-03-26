using ChatSystem.Application.DTOs.chating;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Interfaces
{
    public interface IGroupService
    {
        Task<int> CreateGroupAsync(string creatorId, CreateGroupDto dto);
        Task<GroupDetailsDto> GetGroupDetailsAsync(int chatId);
        Task AddMemberAsync(int chatId, string requesterId, string newUserId);
        Task RemoveMemberAsync(int chatId, string requesterId, string targetUserId);
        Task LeaveGroupAsync(int chatId, string userId);
        Task UpdateGroupInfoAsync(int chatId, string requesterId, string name, IFormFile? image);

    }
}
