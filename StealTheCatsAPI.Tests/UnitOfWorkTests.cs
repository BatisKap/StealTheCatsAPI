using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using StealTheCatsAPI.Application.Database;
using StealTheCatsAPI.Application.Models;
using StealTheCatsAPI.Application.Repository;
using System;

namespace StealTheCatsAPI.Tests
{
    public class UnitOfWorkTests
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;
        private readonly AppDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly Mock<ICatRepository> _catRepositoryMock;
        private readonly Mock<ITagRepository> _tagRepositoryMock;

        public UnitOfWorkTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _context = new AppDbContext(_dbContextOptions);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();

            _catRepositoryMock = new Mock<ICatRepository>();
            _tagRepositoryMock = new Mock<ITagRepository>();
            _unitOfWork = new UnitOfWork(_context, _catRepositoryMock.Object, _tagRepositoryMock.Object);
        }

        [Fact]
        public async Task SaveChangesAsync_Should_SaveChangesToDatabase()
        {
            // Arrange
            var cat = new Cat { CatId = "cat1", Width = 200, Height = 300, Image = "https://example.com/cat1.jpg", Created = DateTime.UtcNow };
            _context.Cats.Add(cat);

            // Act
            var result = await _unitOfWork.SaveChangesAsync();

            // Assert
            Assert.Equal(1, result); // One entity should have been saved
        }

        [Fact]
        public async Task BeginTransactionAsync_Should_BeginDatabaseTransaction()
        {
            // Act
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Assert
            Assert.NotNull(transaction);
            Assert.False(transaction.GetDbTransaction().Connection == null); // Ensure that the transaction is connected to the database
        }

        [Fact]
        public void Dispose_Should_DisposeDbContext()
        {
            // Act
            _unitOfWork.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => _context.Cats.Any());
        }
    }

}
