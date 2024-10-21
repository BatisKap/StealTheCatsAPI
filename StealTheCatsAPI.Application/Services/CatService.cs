using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StealTheCatsAPI.Application.DTO;
using StealTheCatsAPI.Application.Models;
using StealTheCatsAPI.Application.Repository;
using System;
using System.Collections.Concurrent;

namespace StealTheCatsAPI.Application.Services
{
    public class CatService : ICatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        IImageDownloadService _imageDownloadService;
        private readonly string _imageDirectory;

        public CatService(IUnitOfWork unitOfWork, IImageDownloadService imageDownloadService, IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _imageDownloadService = imageDownloadService;

            _imageDirectory = configuration["CatServiceSettings:ImageDirectory"];

            if (!Path.IsPathRooted(_imageDirectory))
                _imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), _imageDirectory);

            if (!Directory.Exists(_imageDirectory))
                Directory.CreateDirectory(_imageDirectory);
        }

        public async Task FetchAndSaveCatsAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("TheCatApiClient");

            string requestUrl = "images/search?size=med&mime_types=jpg&format=json&has_breeds=true&order=RANDOM&page=0&limit=25";

            var uriBuilder = new UriBuilder(httpClient.BaseAddress + requestUrl);

            List<CatApiResponse> cats = new List<CatApiResponse>();
            using (var response = await httpClient.GetAsync(uriBuilder.Uri))
            {
                response.EnsureSuccessStatusCode();
                string stream = await response.Content.ReadAsStringAsync();
                cats = JsonConvert.DeserializeObject<List<CatApiResponse>>(stream);
            }
            var downloadedImagePaths = new List<string>();
            using (IDbContextTransaction transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var allTagsDictionary = new ConcurrentDictionary<string, Tag>();

                    // Get the unique tag names from all cats to reduce DB round trips
                    var allTagNames = cats.SelectMany(cat => cat.Breeds)
                        .SelectMany(breed => breed.Temperament.Split(',').Select(t => t.Trim()))
                        .Where(t => !string.IsNullOrEmpty(t))
                        .Concat(cats.SelectMany(cat => cat.Breeds).Select(breed => breed.Name))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    var existingTags = await _unitOfWork.Tags.GetTagsByNamesAsync(allTagNames);

                    foreach (var existingTag in existingTags)
                        allTagsDictionary.TryAdd(existingTag.Name.ToLower(), existingTag);

                    var newCatEntities = new List<Cat>();

                    foreach (var cat in cats)
                    {
                        if (!await _unitOfWork.Cats.CatExistsAsync(cat.Id))
                        {
                            var catEntity = new Cat
                            {
                                CatId = cat.Id,
                                Width = cat.Width,
                                Height = cat.Height,
                                Image = cat.Url,
                                Created = DateTime.UtcNow,
                                Tags = new List<Tag>()
                            };

                            foreach (var breed in cat.Breeds)
                            {
                                var tags = breed.Temperament.Split(',')
                                    .Select(t => t.Trim())
                                    .Where(t => !string.IsNullOrEmpty(t))
                                    .ToList();

                                tags.Add(breed.Name);

                                foreach (var tagName in tags)
                                {
                                    var normalizedTagName = tagName.ToLower();

                                    if (!allTagsDictionary.TryGetValue(normalizedTagName, out var tagEntity))
                                    {
                                        tagEntity = new Tag
                                        {
                                            Name = tagName,
                                            Created = DateTime.UtcNow
                                        };

                                        allTagsDictionary.TryAdd(normalizedTagName, tagEntity);
                                    }

                                    catEntity.Tags.Add(tagEntity);
                                }
                            }

                            newCatEntities.Add(catEntity);
                        }
                    }

                    await _unitOfWork.Cats.AddCatsAsync(newCatEntities);
                    // Id == 0 means it's not saved to the database yet
                    var newTags = allTagsDictionary.Values.Where(t => t.Id == 0).ToList(); 
                    await _unitOfWork.Tags.AddTagsAsync(newTags);

                    await _unitOfWork.SaveChangesAsync();


                    foreach (var cat in cats)
                    {
                        var destinationPath = Path.Combine(_imageDirectory, $"{cat.Id}.jpg");

                       await _imageDownloadService.SaveImageFromUrlAsync(cat.Url, destinationPath);

                        downloadedImagePaths.Add(destinationPath);
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    foreach (var filePath in downloadedImagePaths)
                    {
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                    }
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    throw; 
                }
            }          
        }

        public async Task<CatDto?> GetCatByIdAsync(int id)
        {
            return await _unitOfWork.Cats.GetCatByIdAsync(id);
        }

        public async Task<IEnumerable<CatDto?>> GetCatsAsync(int page, int pageSize)
        {
            return await _unitOfWork.Cats.GetCatsAsync(page, pageSize);
        }

        public async Task<IEnumerable<CatDto?>> GetCatsByTagAsync(string tag, int page, int pageSize)
        {
            return await _unitOfWork.Cats.GetCatsByTagAsync(tag, page, pageSize);
        }
    }
    public class CatApiResponse
    {
        public string Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Url { get; set; }
        public List<Breed> Breeds { get; set; }

        public class Breed
        {
            public string Name { get; set; }
            public string Temperament { get; set; }
        }
    }
}
