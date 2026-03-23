using ChatSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatSystem.Controllers
{
    [Authorize]
    public class FriendRequestController : Controller
    {
        private readonly IFriendRequestService _friendRequestService;

        public FriendRequestController(IFriendRequestService friendRequestService)
        {
            _friendRequestService = friendRequestService;
        }

        [HttpPost]
        public async Task<IActionResult> Send(string receiverId, string? q)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _friendRequestService.SendRequestAsync(senderId, receiverId);

            return RedirectToAction("Index", "Search", new { q });
        }

        [HttpPost]
        public async Task<IActionResult> Accept(int requestId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _friendRequestService.AcceptRequestAsync(requestId, currentUserId);

            return RedirectToAction("Index", "Friends");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int requestId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _friendRequestService.RejectRequestAsync(requestId, currentUserId);

            return RedirectToAction("Index", "Friends");
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int requestId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _friendRequestService.CancelRequestAsync(requestId, currentUserId);

            return RedirectToAction("Index", "Search");
        }
    }
}
