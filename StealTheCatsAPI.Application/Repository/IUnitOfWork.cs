using Microsoft.EntityFrameworkCore.Storage;
using System;

namespace StealTheCatsAPI.Application.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        ITagRepository Tags { get; }
        ICatRepository Cats { get; }
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
