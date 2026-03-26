using ChatSystem.Application.DTOs.chating;
using ChatSystem.Application.Interfaces;
using ChatSystem.Application.Services;
using ChatSystem.Core.Entities;
using ChatSystem.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatSystem.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IGroupService _groupService;
        private readonly IFriendsService _friendService;
        private readonly IFileUploadService _fileUpload;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _uow;


        public ChatController(IChatService chatService, IGroupService groupService,
            IFriendsService friendService, IFileUploadService fileUpload, IUnitOfWork uow,UserManager<User> userManager)
        {
            _chatService = chatService;
            _groupService = groupService;
            _friendService = friendService;
            _fileUpload = fileUpload;
            _uow = uow;
            _userManager = userManager;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ────────────────── Index ──────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var chats = await _chatService.GetChatListAsync(CurrentUserId);
            return View(chats);
        }

        // ────────────────── Private chat ──────────────────
        [HttpGet]
        public async Task<IActionResult> Open(string friendId)
        {
            var blocked = await _friendService.IsBlockedAsync(CurrentUserId, friendId);
            if (blocked) return View("~/Views/Shared/Blocked.cshtml");

            try
            {
                var chatId = await _chatService.GetOrCreatePrivateChatAsync(CurrentUserId, friendId);
                var messages = await _chatService.GetMessagesAsync(chatId, CurrentUserId);
                await _chatService.MarkMessagesAsReadAsync(chatId, CurrentUserId);

                // ← ضيف الـ friend info
                var friend = await _userManager.FindByIdAsync(friendId);
                ViewBag.FriendName = friend?.UserName ?? "Unknown";
                ViewBag.FriendImage = friend?.ProfileImage;
                ViewBag.FriendStatus = friend?.Status;

                ViewBag.ChatId = chatId;
                ViewBag.FriendId = friendId;
                ViewBag.ChatType = "Private";
                return View("Chat", messages);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // ────────────────── Group chat ──────────────────
        [HttpGet]
        public async Task<IActionResult> Group(int chatId)
        {
            if (!await _chatService.CanAccessChatAsync(CurrentUserId, chatId)) return Forbid();

            var messages = await _chatService.GetMessagesAsync(chatId, CurrentUserId);
            var details = await _groupService.GetGroupDetailsAsync(chatId);
            await _chatService.MarkMessagesAsReadAsync(chatId, CurrentUserId);
            ViewBag.ChatId = chatId;
            ViewBag.ChatType = "Group";
            ViewBag.GroupDetails = details;
            return View("Chat", messages);
        }

        // ────────────────── Create group ──────────────────
        [HttpGet]
        public IActionResult CreateGroup() => View();

        [HttpPost]
        public async Task<IActionResult> CreateGroup(CreateGroupDto dto)
        {
            var chatId = await _groupService.CreateGroupAsync(CurrentUserId, dto);
            return RedirectToAction("Group", new { chatId });
        }

        // ────────────────── File upload ──────────────────
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, int chatId)
        {
            if (!await _chatService.CanAccessChatAsync(CurrentUserId, chatId))
                return Forbid();

            try
            {
                var (url, name) = await _fileUpload.UploadChatFileAsync(file);
                var type = _fileUpload.IsImage(file) ? MessageType.Image : MessageType.File;
                return Json(new { fileUrl = url, fileName = name, type = type.ToString() });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ────────────────── Group management ──────────────────
        [HttpPost]
        public async Task<IActionResult> AddMember(int chatId, string userId)
        {
            await _groupService.AddMemberAsync(chatId, CurrentUserId, userId);
            return RedirectToAction("Group", new { chatId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMember(int chatId, string userId)
        {
            await _groupService.RemoveMemberAsync(chatId, CurrentUserId, userId);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveGroup(int chatId)
        {
            await _groupService.LeaveGroupAsync(chatId, CurrentUserId);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGroup(int chatId, string name, IFormFile? image)
        {
            await _groupService.UpdateGroupInfoAsync(chatId, CurrentUserId, name, image);
            return RedirectToAction("Group", new { chatId });
        }


        [HttpGet]
        public async Task<IActionResult> GetFriendsNotInGroup(int chatId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var existingMembers = (await _uow.ChatUsers.FindAsync(cu => cu.ChatId == chatId))
                .Select(cu => cu.UserId)
                .ToHashSet();

            var friendships = await _uow.Friends.FindAsync(f =>
                (f.UserId == currentUserId || f.FriendId == currentUserId) &&
                f.UserStatus == FriendsStatus.Friend &&
                f.friendsStatus == FriendsStatus.Friend
            );

            var result = new List<object>();

            foreach (var friendship in friendships)
            {
                var friendId = friendship.UserId == currentUserId
                    ? friendship.FriendId
                    : friendship.UserId;

                if (existingMembers.Contains(friendId)) continue;

                var user = await _userManager.FindByIdAsync(friendId);
                if (user is null) continue;

                result.Add(new
                {
                    friendUserId = user.Id,
                    friendUserName = user.UserName,
                    friendProfileImage = user.ProfileImage
                });
            }

            return Json(result);
        }
    }
}
