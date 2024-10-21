using Microsoft.EntityFrameworkCore;
using StealTheCatsAPI.Application.Database;
using StealTheCatsAPI.Application.Models;
using StealTheCatsAPI.Application.Repository;
using System;

namespace StealTheCatsAPI.Tests
{
    public class TagRepositoryTests
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;
        private readonly AppDbContext _context;
        private readonly TagRepository _tagRepository;

        public TagRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _context = new AppDbContext(_dbContextOptions);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();

            _tagRepository = new TagRepository(_context);
        }

        [Fact]
        public async Task AddTagsAsync_Should_AddTagsToDatabase()
        {
            // Arrange
            var tags = new List<Tag>
            {
                new Tag { Name = "playful", Created = DateTime.UtcNow },
                new Tag { Name = "loyal", Created = DateTime.UtcNow }
            };

            // Act
            await _tagRepository.AddTagsAsync(tags, true);

            // Assert
            var savedTags = await _context.Tags.ToListAsync();
            Assert.Equal(2, savedTags.Count);
            Assert.Contains(savedTags, t => t.Name == "playful");
            Assert.Contains(savedTags, t => t.Name == "loyal");
        }

        [Fact]
        public async Task GetTagsByNamesAsync_Should_ReturnMatchingTags()
        {
            // Arrange
            var tags = new List<Tag>
            {
                new Tag { Name = "playful", Created = DateTime.UtcNow },
                new Tag { Name = "loyal", Created = DateTime.UtcNow }
            };

            _context.Tags.AddRange(tags);
            await _context.SaveChangesAsync();

            // Act
            var result = await _tagRepository.GetTagsByNamesAsync(new List<string> { "playful", "loyal" });

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, t => t.Name == "playful");
            Assert.Contains(result, t => t.Name == "loyal");
        }
    }
}
