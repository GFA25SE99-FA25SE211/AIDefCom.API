using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.AppUserRepository
{
    public interface IAppUserRepository
    {
        Task<AppUser> GetUserById(string id);
    }
}
