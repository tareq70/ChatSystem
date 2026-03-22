using ChatSystem.Application.DTOs.User_Profile;
using ChatSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatSystem.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return RedirectToAction("Login", "Auth");

            var profile = await _profileService.GetProfileAsync(userId);
            return View(profile);
        }
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return RedirectToAction("Login", "Auth");

            var profile = await _profileService.GetProfileAsync(userId);

            ViewBag.CurrentImage = profile.ProfileImage;
            ViewBag.Initial = profile.UserName.Substring(0, 1).ToUpper();

            var dto = new UpdateUserProfileDTO
            {
                UserName = profile.UserName
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUserProfileDTO dto)
        {
            if (!ModelState.IsValid)
                return View(dto);


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return RedirectToAction("Login", "Auth");

            var result = await _profileService.UpdateProfileAsync(userId, dto);

            if (!result.IsAuthenticated)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);
                return View(dto);
            }

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}