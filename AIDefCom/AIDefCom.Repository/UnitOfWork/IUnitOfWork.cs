using AIDefCom.Repository.Repositories.AppUserRepository;
using System;
using System.Threading.Tasks;

namespace AIDefCom.Repository.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IAppUserRepository AppUsers { get; }
        Task<int> CompleteAsync();
    }
}
