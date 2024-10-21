using StealTheCatsAPI.Application.DTO;
using StealTheCatsAPI.Application.Models;

namespace StealTheCatsAPI.Application.Repository
{
    public interface ICatRepository
    {
        Task AddCatAsync(Cat cat, bool apply = false);
        Task AddCatsAsync(IEnumerable<Cat?> cats, bool apply = false);
        Task<CatDto?> GetCatByIdAsync(int id);
        Task<IEnumerable<CatDto?>> GetCatsAsync(int page, int pageSize);
        Task<IEnumerable<CatDto?>> GetCatsByTagAsync(string tag, int page, int pageSize);
        Task<bool> CatExistsAsync(string catId);
    }

}
