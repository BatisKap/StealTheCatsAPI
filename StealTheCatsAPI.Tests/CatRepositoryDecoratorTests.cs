using FluentValidation;
using FluentValidation.Results;
using Moq;
using StealTheCatsAPI.Application.Models;
using StealTheCatsAPI.Application.Repository;

namespace StealTheCatsAPI.Tests
{
    public class CatRepositoryDecoratorTests
    {
        private readonly Mock<ICatRepository> _catRepositoryMock;
        private readonly Mock<IValidator<Cat>> _validatorMock;
        private readonly CatRepositoryDecorator _catRepositoryDecorator;

        public CatRepositoryDecoratorTests()
        {
            _catRepositoryMock = new Mock<ICatRepository>();
            _validatorMock = new Mock<IValidator<Cat>>();
            _catRepositoryDecorator = new CatRepositoryDecorator(_catRepositoryMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task AddCatAsync_Should_ValidateCat_BeforeAddingToRepository()
        {
            // Arrange
            var cat = new Cat { CatId = "cat1", Width = 200, Height = 300, Image = "https://example.com/cat1.jpg", Created = DateTime.UtcNow };
            _validatorMock.Setup(v => v.ValidateAsync(cat, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());

            // Act
            await _catRepositoryDecorator.AddCatAsync(cat);

            // Assert
            _validatorMock.Verify(v => v.ValidateAsync(cat, It.IsAny<CancellationToken>()), Times.Once);
            _catRepositoryMock.Verify(r => r.AddCatAsync(cat, false), Times.Once);
        }

        [Fact]
        public async Task AddCatAsync_Should_ThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var cat = new Cat { CatId = "cat1", Width = 200, Height = 300, Image = "https://example.com/cat1.jpg", Created = DateTime.UtcNow };
            var validationFailure = new ValidationFailure("CatId", "CatId is required");
            _validatorMock.Setup(v => v.ValidateAsync(cat, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult(new List<ValidationFailure> { validationFailure }));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _catRepositoryDecorator.AddCatAsync(cat));
            Assert.Contains("CatId is required", exception.Message);
        }

        [Fact]
        public async Task AddCatsAsync_Should_ValidateEachCat_BeforeAddingToRepository()
        {
            // Arrange
            var cats = new List<Cat>
            {
                new Cat { CatId = "cat1", Width = 200, Height = 300, Image = "https://example.com/cat1.jpg", Created = DateTime.UtcNow },
                new Cat { CatId = "cat2", Width = 250, Height = 350, Image = "https://example.com/cat2.jpg", Created = DateTime.UtcNow }
            };
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Cat>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());

            // Act
            await _catRepositoryDecorator.AddCatsAsync(cats);

            // Assert
            _validatorMock.Verify(v => v.ValidateAsync(It.IsAny<Cat>(), It.IsAny<CancellationToken>()), Times.Exactly(cats.Count));
            _catRepositoryMock.Verify(r => r.AddCatsAsync(cats, false), Times.Once);
        }

        [Fact]
        public async Task AddCatsAsync_Should_ThrowValidationException_WhenAnyValidationFails()
        {
            // Arrange
            var cats = new List<Cat>
            {
                new Cat { CatId = "cat1", Width = 200, Height = 300, Image = "https://example.com/cat1.jpg", Created = DateTime.UtcNow },
                new Cat { CatId = "cat2", Width = 250, Height = 350, Image = "https://example.com/cat2.jpg", Created = DateTime.UtcNow }
            };
            var validationFailure = new ValidationFailure("CatId", "CatId is required");
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Cat>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult(new List<ValidationFailure> { validationFailure }));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _catRepositoryDecorator.AddCatsAsync(cats));
            Assert.Contains("CatId is required", exception.Message);
        }
    }
}
