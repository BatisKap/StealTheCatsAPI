using Microsoft.EntityFrameworkCore;
using StealTheCatsAPI.Application.Models;
using System;

namespace StealTheCatsAPI.Application.Repository
{
    public interface ITagRepository
    {
        Task AddTagsAsync(IEnumerable<Tag?> tags, bool apply = false);
        Task<IEnumerable<Tag>> GetTagsByNamesAsync(IEnumerable<string> tagNames);
    }
}
