using ChatSystem.Application.DTOs.Auth;
using ChatSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Register

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _authService.RegisterAsync(dto);

            if (!result.IsAuthenticated)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);
                return View(dto);
            }

            // RegisterAsync already calls SendOtpAsync internally
            // Pass email to VerifyOtp page via TempData
            TempData["Email"] = dto.Email;
            return RedirectToAction(nameof(VerifyOtp));
        }

        // Login

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _authService.LoginAsync(dto);

            if (!result.IsAuthenticated)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);
                return View(dto);
            }

            // Save JWT in HttpOnly cookie
            Response.Cookies.Append("jwt", result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return RedirectToAction("Index", "Profile");
        }

        // Verify OTP  (Register flow + Forgot Password flow)

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            if (TempData["Email"] is not string email)
                return RedirectToAction(nameof(Register));

            // Keep both Email and Flow available for the View and POST
            TempData.Keep("Email");
            TempData.Keep("Flow");

            var dto = new VerifyOtpDto { Email = email };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Email"] = dto.Email;
                TempData.Keep("Flow");
                return View(dto);
            }

            var result = await _authService.VerifyOtpAsync(dto);

            if (!result.IsAuthenticated)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);
                TempData["Email"] = dto.Email;
                TempData.Keep("Flow");
                return View(dto);
            }

            // Route based on which flow triggered VerifyOtp
            if (TempData["Flow"] is "ForgotPassword")
            {
                TempData["Email"] = dto.Email;
                return RedirectToAction(nameof(ResetPassword));
            }

            // Register flow → go to Login with success message
            TempData["Success"] = "Email verified! You can now sign in.";
            return RedirectToAction(nameof(Login));
        }

        // Resend OTP

        [HttpGet]
        public async Task<IActionResult> ResendOtp()
        {
            if (TempData["Email"] is not string email)
                return RedirectToAction(nameof(Register));

            await _authService.SendOtpAsync(email);

            TempData["Email"] = email;
            TempData.Keep("Flow");
            return RedirectToAction(nameof(VerifyOtp));
        }

        // Forgot Password

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _authService.ForgotPasswordAsync(dto);

            if (!result.IsAuthenticated)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);
                return View(dto);
            }

            // Pass email + flow flag so VerifyOtp knows where to redirect after
            TempData["Email"] = dto.Email;
            TempData["Flow"] = "ForgotPassword";
            return RedirectToAction(nameof(VerifyOtp));
        }

        // Reset Password

        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["Email"] is not string email)
                return RedirectToAction(nameof(ForgotPassword));

            TempData.Keep("Email");

            var dto = new ResetPasswordDto { Email = email };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Email"] = dto.Email;
                return View(dto);
            }

            var result = await _authService.ResetPasswordAsync(dto);

            if (!result.IsAuthenticated)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error);
                TempData["Email"] = dto.Email;
                return View(dto);
            }

            TempData["Success"] = "Password reset successfully! You can now sign in.";
            return RedirectToAction(nameof(Login));
        }

        // Logout

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction(nameof(Login));
        }
    }
}