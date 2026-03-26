using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ChatSystem.Application.DTOs.User_Profile
{
    public class UpdateUserProfileDTO
    {

        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string? UserName { get; set; } = string.Empty;

        public IFormFile? ProfileImage { get; set; }
    }
}
