using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Application.Interfaces
{
    public interface IFileUploadService
    {
        Task<(string FileUrl, string FileName)> UploadChatFileAsync(IFormFile file);
        bool IsImage(IFormFile file);
    }
}
