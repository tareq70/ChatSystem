using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public bool IsAuthenticated { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
        public string[] Errors { get; set; } = Array.Empty<string>();
    }
}
