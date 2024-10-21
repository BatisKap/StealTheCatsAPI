using Microsoft.EntityFrameworkCore;
using StealTheCatsAPI.Application.Database;
using StealTheCatsAPI.Application.DTO;
using StealTheCatsAPI.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StealTheCatsAPI.Application.Repository
{
    public class CatRepository : ICatRepository
    {
        private readonly AppDbContext _context;

        public CatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddCatAsync(Cat cat, bool apply = false)
        {
            await _context.Cats.AddAsync(cat);
            if (apply)
                await _context.SaveChangesAsync();
        }
        public async Task AddCatsAsync(IEnumerable<Cat?> cats, bool apply = false)
        {
            await _context.Cats.AddRangeAsync(cats);
            if (apply)
                await _context.SaveChangesAsync();
        }
        public async Task<CatDto?> GetCatByIdAsync(int id)
        {
            var cat = await _context.Cats
                .Include(c => c.Tags)
                .FirstOrDefaultAsync(c => c.Id == id);
            return MapToDTo(cat);
        }

        public async Task<IEnumerable<CatDto?>> GetCatsAsync(int page, int pageSize)
        {
            var cats = await _context.Cats
                .Include(c => c.Tags)
                .OrderByDescending(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return cats.Select(c => MapToDTo(c));
        }

        public async Task<IEnumerable<CatDto?>> GetCatsByTagAsync(string tag, int page, int pageSize)
        {
            var cats = await _context.Cats
                .Include(c => c.Tags)
                .Where(c => c.Tags.Any(t => EF.Functions.Like(t.Name, tag)))
                .OrderByDescending(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return cats.Select(c => MapToDTo(c));
        }
        public async Task<bool> CatExistsAsync(string catId)
        {
            return await _context.Cats.AnyAsync(c => c.CatId == catId);
        }

        private CatDto MapToDTo(Cat c)
        {
            if (c == null)
                return null;
            return new CatDto
            {
                Id = c.Id,
                CatId = c.CatId,
                Width = c.Width,
                Height = c.Height,
                Image = c.Image,
                Created = c.Created,
                Tags = c.Tags?.Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name
                }).ToList()
            };
        }
    }

}
