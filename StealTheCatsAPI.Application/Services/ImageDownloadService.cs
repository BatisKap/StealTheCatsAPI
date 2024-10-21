using System;

namespace StealTheCatsAPI.Application.Services
{
    public class ImageDownloadService : IImageDownloadService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ImageDownloadService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task SaveImageFromUrlAsync(string imageUrl, string destinationPath)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ImageDownloadClient");

                var response = await httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                var imageBytes = await response.Content.ReadAsByteArrayAsync();

                await File.WriteAllBytesAsync(destinationPath, imageBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download or save image from URL {imageUrl}: {ex.Message}");
            }
        }
    }
}
