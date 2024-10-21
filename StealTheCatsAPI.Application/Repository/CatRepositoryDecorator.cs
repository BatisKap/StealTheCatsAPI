using FluentValidation;
using StealTheCatsAPI.Application.DTO;
using StealTheCatsAPI.Application.Models;
using System;

namespace StealTheCatsAPI.Application.Repository
{
    public class CatRepositoryDecorator : ICatRepository
    {
        private readonly ICatRepository _decorated;
        private readonly IValidator<Cat> _validator;

        public CatRepositoryDecorator(ICatRepository innerRepository, IValidator<Cat> validator)
        {
            _decorated = innerRepository;
            _validator = validator;
        }
        public async Task AddCatAsync(Cat cat, bool apply = false)
        {
            var validationResult = await _validator.ValidateAsync(cat);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }

            await _decorated.AddCatAsync(cat);
        }
        public async Task AddCatsAsync(IEnumerable<Cat?> cats, bool apply = false)
        {
            foreach (var cat in cats)
            {
                var validationResult = await _validator.ValidateAsync(cat);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    throw new ValidationException($"Validation failed: {errors}");
                }
            }
            await _decorated.AddCatsAsync(cats);
        }

        public async Task<IEnumerable<CatDto?>> GetCatsAsync(int page, int pageSize)
        {
            return await _decorated.GetCatsAsync(page, pageSize);
        }
        public async Task<IEnumerable<CatDto?>> GetCatsByTagAsync(string tag, int page, int pageSize)
        {
            return await _decorated.GetCatsByTagAsync(tag, page, pageSize);
        }
        public async Task<CatDto?> GetCatByIdAsync(int id)
        {
            return await _decorated.GetCatByIdAsync(id);
        }

        public async Task<bool> CatExistsAsync(string catId)
        {
            return await _decorated.CatExistsAsync(catId);
        }
    }
}
