using ChatSystem.Core.Entities;
using ChatSystem.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace ChatSystem.Infrastructure.Hubs
{
    public class PresenceHub : Hub
    {
        private readonly UserManager<User> _userManager;

        public PresenceHub(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is not null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user is not null)
                {
                    user.Status = UserStatus.Online;
                    user.LastSeen = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    await Clients.Others.SendAsync("UserOnline", userId);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is not null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user is not null)
                {
                    user.Status = UserStatus.Offline;
                    user.LastSeen = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    await Clients.Others.SendAsync("UserOffline", userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
