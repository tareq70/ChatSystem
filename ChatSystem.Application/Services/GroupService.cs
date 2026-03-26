using ChatSystem.Application.DTOs.chating;
using ChatSystem.Application.Interfaces;
using ChatSystem.Core.Entities;
using ChatSystem.Core.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Services
{
    public class GroupService : IGroupService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _env;

        public GroupService(IUnitOfWork uow, UserManager<User> userManager, IWebHostEnvironment env)
        {
            _uow = uow;
            _userManager = userManager;
            _env = env;
        }

        public async Task<int> CreateGroupAsync(string creatorId, CreateGroupDto dto)
        {
            var chat = new Chat { Type = ChatType.Group };
            await _uow.Chats.AddAsync(chat);
            await _uow.SaveAsync();

            string? imageUrl = null;
            if (dto.GroupImage is not null)
                imageUrl = await SaveGroupImageAsync(dto.GroupImage);

            var group = new Group
            {
                ChatId = chat.Id,
                Name = dto.Name.Trim(),
                GroupImage = imageUrl,
                CreatedByUserId = creatorId
            };
            await _uow.Groups.AddAsync(group);

            await _uow.ChatUsers.AddAsync(new ChatUser { ChatId = chat.Id, UserId = creatorId });

            foreach (var memberId in dto.MemberIds.Distinct().Where(id => id != creatorId))
                await _uow.ChatUsers.AddAsync(new ChatUser { ChatId = chat.Id, UserId = memberId });

            await _uow.SaveAsync();
            return chat.Id;
        }

        public async Task<GroupDetailsDto> GetGroupDetailsAsync(int chatId)
        {
            var group = (await _uow.Groups.FindAsync(g => g.ChatId == chatId)).First();
            var members = (await _uow.ChatUsers.FindAsync(cu => cu.ChatId == chatId)).ToList();

            var memberDtos = new List<GroupMemberDto>();
            foreach (var cu in members)
            {
                var user = await _userManager.FindByIdAsync(cu.UserId);
                if (user is null) continue;
                memberDtos.Add(new GroupMemberDto
                {
                    UserId = user.Id,
                    UserName = user.UserName!,
                    ProfileImage = user.ProfileImage,
                    Status = user.Status,
                    IsAdmin = user.Id == group.CreatedByUserId
                });
            }

            return new GroupDetailsDto
            {
                ChatId = chatId,
                Name = group.Name,
                GroupImage = group.GroupImage,
                CreatedByUserId = group.CreatedByUserId,
                Members = memberDtos
            };
        }

        public async Task AddMemberAsync(int chatId, string requesterId, string newUserId)
        {
            var group = (await _uow.Groups.FindAsync(g => g.ChatId == chatId)).First();
            if (group.CreatedByUserId != requesterId)
                throw new UnauthorizedAccessException("Only admin can add members.");

            var alreadyIn = (await _uow.ChatUsers.FindAsync(cu => cu.ChatId == chatId && cu.UserId == newUserId)).Any();
            if (!alreadyIn)
            {
                await _uow.ChatUsers.AddAsync(new ChatUser { ChatId = chatId, UserId = newUserId });
                await _uow.SaveAsync();
            }
        }

        public async Task RemoveMemberAsync(int chatId, string requesterId, string targetUserId)
        {
            var group = (await _uow.Groups.FindAsync(g => g.ChatId == chatId)).First();
            if (group.CreatedByUserId != requesterId)
                throw new UnauthorizedAccessException("Only admin can remove members.");

            var cu = (await _uow.ChatUsers.FindAsync(c => c.ChatId == chatId && c.UserId == targetUserId)).FirstOrDefault();
            if (cu is not null)
            {
                _uow.ChatUsers.Delete(cu);
                await _uow.SaveAsync();
            }
        }

        public async Task LeaveGroupAsync(int chatId, string userId)
        {
            var cu = (await _uow.ChatUsers.FindAsync(c => c.ChatId == chatId && c.UserId == userId)).FirstOrDefault();
            if (cu is not null)
            {
                _uow.ChatUsers.Delete(cu);
                await _uow.SaveAsync();
            }
        }

        public async Task UpdateGroupInfoAsync(int chatId, string requesterId, string name, IFormFile? image)
        {
            var group = (await _uow.Groups.FindAsync(g => g.ChatId == chatId)).First();
            if (group.CreatedByUserId != requesterId)
                throw new UnauthorizedAccessException("Only admin can update group info.");

            group.Name = name.Trim();
            if (image is not null)
                group.GroupImage = await SaveGroupImageAsync(image);

            _uow.Groups.Update(group);
            await _uow.SaveAsync();
        }

        private async Task<string> SaveGroupImageAsync(IFormFile file)
        {
            var folder = Path.Combine(_env.WebRootPath, "uploads", "group-images");
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(folder, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/group-images/{fileName}";
        }
    }
}
