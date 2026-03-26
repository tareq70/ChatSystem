using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.DTOs.chating
{
    public class CreateGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile? GroupImage { get; set; }
        public List<string> MemberIds { get; set; } = new();
    }
}
