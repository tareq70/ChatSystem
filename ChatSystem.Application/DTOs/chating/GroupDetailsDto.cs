using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.DTOs.chating
{
    public class GroupDetailsDto
    {
        public int ChatId { get; set; }
        public string Name { get; set; }
        public string? GroupImage { get; set; }
        public string CreatedByUserId { get; set; }
        public List<GroupMemberDto> Members { get; set; } = new();
    }
}
