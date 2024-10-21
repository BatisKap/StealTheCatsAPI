using Microsoft.EntityFrameworkCore;
using StealTheCatsAPI.Application.Database;
using StealTheCatsAPI.Application.Models;
using StealTheCatsAPI.Application.Repository;
using System;

namespace StealTheCatsAPI.Tests
{
    public class CatRepositoryTests
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;
        private readonly AppDbContext _context;
        private readonly CatRepository _catRepository;

        public CatRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _context = new AppDbContext(_dbContextOptions);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();

            _catRepository = new CatRepository(_context);
        }

        [Fact]
        public async Task AddCatAsync_Should_AddCatToDatabase()
        {
            // Arrange
            var cat = new Cat
            {
                CatId = "cat1",
                Width = 200,
                Height = 300,
                Image = "https://example.com/cat1.jpg",
                Created = DateTime.UtcNow
            };

            // Act
            await _catRepository.AddCatAsync(cat, true);

            // Assert
            var savedCat = await _context.Cats.FirstOrDefaultAsync(c => c.CatId == "cat1");
            Assert.NotNull(savedCat);
            Assert.Equal(cat.CatId, savedCat.CatId);
        }

        [Fact]
        public async Task GetCatByIdAsync_Should_ReturnCatDto_WhenCatExists()
        {
            // Arrange
            var cat = new Cat
            {
                Id = 1,
                CatId = "cat1",
                Width = 200,
                Height = 300,
                Image = "https://example.com/cat1.jpg",
                Created = DateTime.UtcNow,
                Tags = new List<Tag>()
            };

            _context.Cats.Add(cat);
            await _context.SaveChangesAsync();

            // Act
            var result = await _catRepository.GetCatByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cat.CatId, result.CatId);
            Assert.Equal(cat.Width, result.Width);
            Assert.Equal(cat.Height, result.Height);
        }

        [Fact]
        public async Task GetCatsAsync_Should_ReturnPagedCats()
        {
            // Arrange
            var cats = new List<Cat>
            {
                new Cat { Id = 1, CatId = "cat1", Width = 200, Height = 300, Image = "https://example.com/cat1.jpg", Created = DateTime.UtcNow },
                new Cat { Id = 2, CatId = "cat2", Width = 250, Height = 350, Image = "https://example.com/cat2.jpg", Created = DateTime.UtcNow }
            };

            _context.Cats.AddRange(cats);
            await _context.SaveChangesAsync();

            // Act
            var result = await _catRepository.GetCatsAsync(1, 2);

            // Assert
            Assert.Equal(2, result.Count());
        }
    }
}
