using Moq;
using StealTheCatsAPI.Application.Models;
using StealTheCatsAPI.Application.Repository;
using StealTheCatsAPI.Application.Services;
using Microsoft.Extensions.Configuration;
using Moq.Protected;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using System.Net;

namespace StealTheCatsAPI.Tests
{
    public class CatServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IImageDownloadService> _imageDownloadServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly CatService _catService;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly string _imageDirectory;

        public CatServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _imageDownloadServiceMock = new Mock<IImageDownloadService>();
            _configurationMock = new Mock<IConfiguration>();

            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new System.Uri("https://api.thecatapi.com/v1/")
            };
            _httpClientFactoryMock.Setup(factory => factory.CreateClient("TheCatApiClient")).Returns(_httpClient);

            _imageDirectory = "TestImages";
            _configurationMock.Setup(config => config["CatServiceSettings:ImageDirectory"]).Returns(_imageDirectory);

            _catService = new CatService(_unitOfWorkMock.Object, _imageDownloadServiceMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);
        }

        [Fact]
        public async Task FetchAndSaveCatsAsync_Should_FetchCatsAndSaveThemToDatabase()
        {
            // Arrange
            var catApiResponse = new List<CatApiResponse>
            {
                new CatApiResponse
                {
                    Id = "cat1",
                    Width = 200,
                    Height = 300,
                    Url = "https://example.com/cat1.jpg",
                    Breeds = new List<CatApiResponse.Breed>
                    {
                        new CatApiResponse.Breed { Name = "Breed1", Temperament = "Playful, Loyal" }
                    }
                },
                new CatApiResponse
                {
                    Id = "cat2",
                    Width = 250,
                    Height = 350,
                    Url = "https://example.com/cat2.jpg",
                    Breeds = new List<CatApiResponse.Breed>
                    {
                        new CatApiResponse.Breed { Name = "Breed2", Temperament = "Friendly, Energetic" }
                    }
                }
            };
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(catApiResponse))
            };
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("images/search")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            _unitOfWorkMock.Setup(uow => uow.BeginTransactionAsync()).ReturnsAsync(Mock.Of<IDbContextTransaction>());
            _unitOfWorkMock.Setup(uow => uow.Cats.CatExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _unitOfWorkMock.Setup(uow => uow.Tags.GetTagsByNamesAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Tag>());

            // Act
            await _catService.FetchAndSaveCatsAsync();

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Cats.AddCatsAsync(It.IsAny<IEnumerable<Cat>>(), false), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.Tags.AddTagsAsync(It.IsAny<IEnumerable<Tag>>(), false), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
            _imageDownloadServiceMock.Verify(ids => ids.SaveImageFromUrlAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(catApiResponse.Count));
        }

        [Fact]
        public async Task FetchAndSaveCatsAsync_Should_RollbackTransaction_OnError()
        {
            // Arrange
            var catApiResponse = new List<CatApiResponse>
            {
                new CatApiResponse
                {
                    Id = "cat1",
                    Width = 200,
                    Height = 300,
                    Url = "https://example.com/cat1.jpg",
                    Breeds = new List<CatApiResponse.Breed>
                    {
                        new CatApiResponse.Breed { Name = "Breed1", Temperament = "Playful, Loyal" }
                    }
                }
            };
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(catApiResponse))
            };

            // Mock the HTTP response for cat data fetching
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("images/search")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Mock the transaction behavior to test rollback
            var transactionMock = new Mock<IDbContextTransaction>();
            transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            transactionMock.Setup(t => t.Dispose()).Verifiable();

            _unitOfWorkMock.Setup(uow => uow.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

            // Mock repository behaviors, setting the expectation that adding cats will throw an exception
            _unitOfWorkMock.Setup(uow => uow.Cats.CatExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _unitOfWorkMock.Setup(uow => uow.Tags.GetTagsByNamesAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Tag>());
            _unitOfWorkMock.Setup(uow => uow.Cats.AddCatsAsync(It.IsAny<IEnumerable<Cat>>(), false)).ThrowsAsync(new System.Exception("Test Exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.Exception>(() => _catService.FetchAndSaveCatsAsync());
            Assert.Equal("Test Exception", exception.Message);

            // Verify that the transaction is rolled back after the exception
            transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            transactionMock.Verify(t => t.Dispose(), Times.Once);
        }


        [Fact]
        public async Task FetchAndSaveCatsAsync_Should_CreateDirectory_WhenImageDirectoryDoesNotExist()
        {
            // Arrange
            var tempImageDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _configurationMock.Setup(config => config["CatServiceSettings:ImageDirectory"]).Returns(tempImageDirectory);

            var catApiResponse = new List<CatApiResponse>
            {
                new CatApiResponse
                {
                    Id = "cat1",
                    Width = 200,
                    Height = 300,
                    Url = "https://example.com/cat1.jpg",
                    Breeds = new List<CatApiResponse.Breed>
                    {
                        new CatApiResponse.Breed { Name = "Breed1", Temperament = "Playful, Loyal" }
                    }
                }
            };

            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(catApiResponse))
            };

            // Mock the HttpClient response to return the cat API data
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("images/search")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Set up the mocks to ensure they provide the necessary transactions and cats
            var transactionMock = new Mock<IDbContextTransaction>();
            _unitOfWorkMock.Setup(uow => uow.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
            _unitOfWorkMock.Setup(uow => uow.Cats.CatExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _unitOfWorkMock.Setup(uow => uow.Tags.GetTagsByNamesAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Tag>());

            var catService = new CatService(_unitOfWorkMock.Object, _imageDownloadServiceMock.Object, _configurationMock.Object, _httpClientFactoryMock.Object);

            // Act
            await catService.FetchAndSaveCatsAsync();

            // Assert
            Assert.True(Directory.Exists(tempImageDirectory));

            // Clean up
            if (Directory.Exists(tempImageDirectory))
                Directory.Delete(tempImageDirectory, true);
        }
    }
}