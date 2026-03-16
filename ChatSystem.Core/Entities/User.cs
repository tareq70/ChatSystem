using ChatSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Core.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; } 

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string? ProfileImage { get; set; }

        public UserStatus Status { get; set; } 

        public DateTime CreatedAt { get; set; }
    }
}
