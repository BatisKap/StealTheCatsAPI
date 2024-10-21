using StealTheCatsAPI.Application.DTO;
using System;

namespace StealTheCatsAPI.Application.Services
{
    public interface ICatService
    {
        Task FetchAndSaveCatsAsync();
        Task<CatDto?> GetCatByIdAsync(int id);
        Task<IEnumerable<CatDto?>> GetCatsAsync(int page, int pageSize);
        Task<IEnumerable<CatDto?>> GetCatsByTagAsync(string tag, int page, int pageSize);
    }
}
