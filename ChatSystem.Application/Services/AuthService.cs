using ChatSystem.Application.DTOs.Auth;
using ChatSystem.Application.Interfaces;
using ChatSystem.Core.Entities;
using ChatSystem.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<User> userManager, IConfiguration config, IEmailService emailService)
        {
            _userManager = userManager;
            _config = config;
            _emailService = emailService;
        }
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = new[] { "Email is already registered." }
                };

            var existingUsername = await _userManager.FindByNameAsync(registerDto.UserName);
            if (existingUsername != null)
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = new[] { "Username is already taken." }
                };

            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                Status = UserStatus.Offline,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = result.Errors.Select(e => e.Description).ToArray()
                };

            await _userManager.AddToRoleAsync(user, Roles.User);

            // Send OTP — no token until email is verified
            await SendOtpAsync(user.Email!);

            return new AuthResponseDto
            {
                IsAuthenticated = true,
                Username = user.UserName!,
                Roles = new[] { Roles.User }
            };
        }

        // ---------------------------------------------------------------
        // Login
        // ---------------------------------------------------------------

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = loginDto.UsernameOrEmail.Contains('@')
                ? await _userManager.FindByEmailAsync(loginDto.UsernameOrEmail)
                : await _userManager.FindByNameAsync(loginDto.UsernameOrEmail);

            if (user is null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = new[] { "Invalid username or password." }
                };

            if (!user.EmailConfirmed)
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = new[] { "Please verify your email before signing in." }
                };

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            return new AuthResponseDto
            {
                IsAuthenticated = true,
                Username = user.UserName!,
                Token = token,
                Roles = roles.ToArray()
            };
        }

        // ---------------------------------------------------------------
        // Send OTP
        // ---------------------------------------------------------------

        public async Task<AuthResponseDto> SendOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = new[] { "Email not found." }
                };

            var otp = new Random().Next(100000, 999999).ToString();

            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            await _userManager.UpdateAsync(user);

            await _emailService.SendEmailAsync(
                email,
                "Your verification code",
                $"""
                <div style="font-family:sans-serif;max-width:400px;margin:auto;padding:24px;border:1px solid #e2e8f0;border-radius:12px;">
                    <h2 style="color:#0f172a;">Your verification code</h2>
                    <p style="color:#64748b;">Use the code below to verify your email. Valid for <strong>10 minutes</strong>.</p>
                    <div style="font-size:32px;font-weight:800;letter-spacing:8px;color:#1d4ed8;margin:24px 0;">{otp}</div>
                    <p style="color:#94a3b8;font-size:12px;">If you didn't request this, you can ignore this email.</p>
                </div>
                """
            );

            return new AuthResponseDto { IsAuthenticated = true };
        }

        // ---------------------------------------------------------------
        // Verify OTP
        // ---------------------------------------------------------------

        public async Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = new[] { "Email not found." }
                };

            if (user.OtpCode != dto.OtpCode || user.OtpExpiry < DateTime.UtcNow)
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = new[] { "Invalid or expired OTP." }
                };

            // Confirm email + clear OTP
            user.EmailConfirmed = true;
            user.OtpCode = null;
            user.OtpExpiry = null;
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto { IsAuthenticated = true };
        }

        // ---------------------------------------------------------------
        // Forgot Password
        // ---------------------------------------------------------------

        public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            return await SendOtpAsync(dto.Email);
        }

        // ---------------------------------------------------------------
        // Reset Password — OTP already verified in VerifyOtpAsync
        // ---------------------------------------------------------------

        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = new[] { "Email not found." }
                };

            // OTP was already verified — go straight to reset
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!result.Succeeded)
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Errors = result.Errors.Select(e => e.Description).ToArray()
                };

            return new AuthResponseDto { IsAuthenticated = true };
        }

        // ---------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------

        private string GenerateJwtToken(User user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name,           user.UserName!),
                new(ClaimTypes.Email,          user.Email!)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}