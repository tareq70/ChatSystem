using ChatSystem.Application.DTOs.chating;
using ChatSystem.Application.Interfaces;
using ChatSystem.Core.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Services
{
    public class FileUploadService: IFileUploadService
    {
        private readonly IWebHostEnvironment _env;
        private static readonly string[] _imageTypes = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public FileUploadService(IWebHostEnvironment env) => _env = env;

        public async Task<(string FileUrl, string FileName)> UploadChatFileAsync(IFormFile file)
        {
            if (file.Length > MaxFileSize)
                throw new InvalidOperationException("File exceeds 10MB limit.");

            var folder = Path.Combine(_env.WebRootPath, "uploads", "chat-files");
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName).ToLower();
            var saveName = $"{Guid.NewGuid()}{ext}";
            var path = Path.Combine(folder, saveName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return ($"/uploads/chat-files/{saveName}", file.FileName);
        }

        public bool IsImage(IFormFile file)
            => _imageTypes.Contains(Path.GetExtension(file.FileName).ToLower());
    }
}
