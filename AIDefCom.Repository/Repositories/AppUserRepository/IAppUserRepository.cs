using AIDefCom.Repository.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.AppUserRepository
{
    public interface IAppUserRepository
    {
        Task<IEnumerable<AppUser>> GetAllUsersAsync();

        Task<AppUser?> GetUserByIdAsync(string id);

        Task<AppUser?> GetUserByEmailAsync(string email);
    }
}
