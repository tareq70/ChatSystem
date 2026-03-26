using ChatSystem.Application.DTOs.Search;
using ChatSystem.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Interfaces
{
    public interface IUserSearchService
    {
        Task<List<UserSearchResultDto>> SearchUsersAsync(string query, string currentUserId);
    }
}
