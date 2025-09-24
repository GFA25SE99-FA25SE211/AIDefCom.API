using AIDefCom.Repository.Repositories;
using AIDefCom.Repository.Repositories.AppUserRepository;
using System.Threading.Tasks;

namespace AIDefCom.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IAppUserRepository AppUsers { get; }

        public UnitOfWork(ApplicationDbContext context, IAppUserRepository appUserRepository)
        {
            _context = context;
            AppUsers = appUserRepository;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
