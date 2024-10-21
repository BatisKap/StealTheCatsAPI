using System;
using System.Collections.Generic;

namespace StealTheCatsAPI.Application.Services
{
    public interface IImageDownloadService
    {
        Task SaveImageFromUrlAsync(string imageUrl, string destinationPath);
    }
}
