using Microsoft.EntityFrameworkCore;
using StealTheCatsAPI.Application.Database;
using StealTheCatsAPI.Application.Models;
using System;

namespace StealTheCatsAPI.Application.Repository
{
    public class TagRepository : ITagRepository
    {
        private readonly AppDbContext _context;

        public TagRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddTagsAsync(IEnumerable<Tag?> tags, bool apply = false)
        {
            await _context.Tags.AddRangeAsync(tags);
            if (apply)
                await _context.SaveChangesAsync();
        }      
        public async Task<IEnumerable<Tag>> GetTagsByNamesAsync(IEnumerable<string> tagNames)
        {
            return await _context.Tags
                .Where(tag => tagNames.Contains(tag.Name))
                .ToListAsync();
        }    
    }
}
