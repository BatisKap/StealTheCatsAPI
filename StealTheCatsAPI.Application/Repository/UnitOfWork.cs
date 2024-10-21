using Microsoft.EntityFrameworkCore.Storage;
using StealTheCatsAPI.Application.Database;
using System;

namespace StealTheCatsAPI.Application.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context, ICatRepository catRepository, ITagRepository tagRepository)
        {
            _context = context;
            Cats = catRepository; 
            Tags = tagRepository;
        }

        public ITagRepository Tags { get; private set; }
        public ICatRepository Cats { get; private set; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
