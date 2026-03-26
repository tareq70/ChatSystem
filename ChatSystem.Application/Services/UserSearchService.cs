using ChatSystem.Application.DTOs.Search;
using ChatSystem.Application.Interfaces;
using ChatSystem.Core.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Services
{
    public class UserSearchService : IUserSearchService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _uow;

        public UserSearchService(UserManager<User> userManager, IUnitOfWork uow)
        {
            _userManager = userManager;
            _uow = uow;
        }


        public async Task<List<UserSearchResultDto>> SearchUsersAsync(string query, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<UserSearchResultDto>();


            var users = _userManager.Users
                .Where(u => u.Id != currentUserId &&
                            (u.UserName.Contains(query) || u.Email.Contains(query)))
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.ProfileImage,
                    u.LastSeen,
                    u.Status
                })
                .Take(20)
                .ToList();

            var result = new List<UserSearchResultDto>();

            foreach (var user in users)
            {
                var requests = await _uow.FriendRequests.FindAsync(r =>
                    (r.SenderId == currentUserId && r.ReceiverId == user.Id) ||
                    (r.SenderId == user.Id && r.ReceiverId == currentUserId));

                var request = requests.FirstOrDefault();

                result.Add(new UserSearchResultDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    ProfileImage = user.ProfileImage,  
                    Status = user.Status,
                    LastSeen = user.LastSeen,
                    RequestStatus = request?.Status
                });
            }

            return result;
        }
    }
}