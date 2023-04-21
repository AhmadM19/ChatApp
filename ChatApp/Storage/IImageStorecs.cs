﻿using ChatApp.Dtos;
using System.Linq.Expressions;

namespace ChatApp.Storage
{
    public interface IImageStore
    {
        Task<string> UploadImage(IFormFile file);
        Task<byte[]> DownloadImage(string id);
    }
}