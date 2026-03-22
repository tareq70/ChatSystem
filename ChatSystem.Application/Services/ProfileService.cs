using ChatSystem.Application.DTOs.Auth;
using ChatSystem.Application.DTOs.User_Profile;
using ChatSystem.Application.Interfaces;
using ChatSystem.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Services
{
    public class ProfileService : IProfileService 
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public ProfileService(UserManager<User> userManager,IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;

        }

        public async Task<UserProfileDTO> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                throw new Exception("User not found.");

            var friendsCount = await _unitOfWork.Friends
                .GetQueryable()
                .CountAsync(f => f.UserId == userId || f.FriendId == userId);

            return new UserProfileDTO
            {
                UserName = user.UserName!,
                Email = user.Email!,
                ProfileImage = user.ProfileImage,
                Status = user.Status,
                LastSeen = user.LastSeen ?? DateTime.UtcNow,
                CreatedAt = user.CreatedAt,
                FriendsCount = friendsCount
            };
        }
        public async Task<AuthResponseDto> UpdateProfileAsync(string userId, UpdateUserProfileDTO dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = new[] { "User not found." }
                };

            // 1. Check if username taken by another user
            var existingUser = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUser != null && existingUser.Id != userId)
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = new[] { "Username is already taken." }
                };

            // 2. Update username
            user.UserName = dto.UserName;

            // 3. Handle profile image upload
            if (dto.ProfileImage != null && dto.ProfileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile-images");
                Directory.CreateDirectory(uploadsFolder);

                // Delete old image if exists
                if (!string.IsNullOrEmpty(user.ProfileImage))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfileImage.TrimStart('/'));
                    if (File.Exists(oldPath))
                        File.Delete(oldPath);
                }

                var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(dto.ProfileImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.ProfileImage.CopyToAsync(stream);

                user.ProfileImage = $"/uploads/profile-images/{fileName}";
            }

            // 4. Save changes
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = result.Errors.Select(e => e.Description).ToArray()
                };

            return new AuthResponseDto
            {
                IsAuthenticated = true,
                Username = user.UserName!
            };
        }
    }
}
