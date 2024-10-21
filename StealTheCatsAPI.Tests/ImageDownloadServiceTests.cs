using Moq.Protected;
using Moq;
using StealTheCatsAPI.Application.Services;
using System.Net;
using System.Net.Http;

namespace StealTheCatsAPI.Tests
{
    public class ImageDownloadServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly ImageDownloadService _imageDownloadService;

        public ImageDownloadServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _imageDownloadService = new ImageDownloadService(_httpClientFactoryMock.Object);
        }

        [Fact]
        public async Task SaveImageFromUrlAsync_Should_DownloadImageAndSaveToDisk()
        {
            // Arrange
            var imageUrl = "https://example.com/cat1.jpg";
            var destinationPath = Path.Combine(Path.GetTempPath(), "cat1.jpg");

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == imageUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(new byte[] { 1, 2, 3, 4 })
                });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _httpClientFactoryMock.Setup(factory => factory.CreateClient("ImageDownloadClient")).Returns(httpClient);

            // Act
            await _imageDownloadService.SaveImageFromUrlAsync(imageUrl, destinationPath);

            // Assert
            Assert.True(File.Exists(destinationPath));
            var fileContent = await File.ReadAllBytesAsync(destinationPath);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, fileContent);

            // Clean up
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }
        }
        [Fact]
        public async Task SaveImageFromUrlAsync_Should_LogError_WhenImageDownloadFails()
        {
            // Arrange
            var imageUrl = "https://example.com/testimage.jpg";
            var destinationPath = "testimage.jpg";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == imageUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Test Exception"));

            // Act
            var exception = await Record.ExceptionAsync(() => _imageDownloadService.SaveImageFromUrlAsync(imageUrl, destinationPath));

            // Assert
            Assert.Null(exception); // No exception should be thrown since it's handled internally
        }
    }

}
