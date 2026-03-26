using ChatSystem.Application.DTOs.Auth;
using ChatSystem.Application.DTOs.User_Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfileDTO> GetProfileAsync(string userId);
        Task<AuthResponseDto> UpdateProfileAsync(string userId, UpdateUserProfileDTO dto);
    }
}
