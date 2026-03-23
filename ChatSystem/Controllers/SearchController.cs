using ChatSystem.Application.DTOs.Search;
using ChatSystem.Application.Interfaces;
using ChatSystem.Application.Services;
using ChatSystem.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatSystem.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly IUserSearchService _searchService;
        private readonly UserManager<User> _userManager;
        private readonly IProfileService _profileService;
        private readonly IFriendsService _friendsService;

        public SearchController(IUserSearchService searchService, UserManager<User> userManager, IProfileService profileService, IFriendsService friendsService)
        {
            _searchService = searchService;
            _userManager = userManager;
            _profileService = profileService;
            _friendsService = friendsService;
        }

        public async Task<IActionResult> Index(string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return View(new List<UserSearchResultDto>());

            var currentUserId = _userManager.GetUserId(User)!;
            var results = await _searchService.SearchUsersAsync(q, currentUserId);

            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> ShowProfile(string id)
        {
            if (id is null)
                return RedirectToAction("Login", "Auth");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isBlocked = await _friendsService.IsBlockedAsync(currentUserId, id);

            if (isBlocked)
            {
                return View("Blocked");
            }

            var profile = await _profileService.GetProfileAsync(id);
            return View(profile);
        }

    }
}