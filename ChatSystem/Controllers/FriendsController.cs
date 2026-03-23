using ChatSystem.Application.DTOs.Friends;
using ChatSystem.Application.Interfaces;
using ChatSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatSystem.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        private readonly IFriendsService _friendsService;

        public FriendsController(IFriendsService friendsService)
        {
            _friendsService = friendsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var model = new FriendsPageDto
            {
                PendingRequests = await _friendsService.GetPendingRequestsAsync(userId),
                MyFriends = await _friendsService.GetMyFriendsAsync(userId)
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Remove(string friendId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _friendsService.RemoveFriendAsync(userId, friendId);
            return RedirectToAction("Index", "Friends");
        }

        [HttpPost]
        public async Task<IActionResult> Block(string blockedId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _friendsService.BlockUserAsync(userId, blockedId);
            return RedirectToAction("Index", "Friends");
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(string blockedId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var isBlocker = await _friendsService.IsBlockerAsync(userId, blockedId);
            if (!isBlocker)
                return RedirectToAction("Index", "Friends");

            await _friendsService.UnblockUserAsync(userId, blockedId);
            return RedirectToAction("Index", "Friends");
        }

    }
}
