using AIDefCom.Repository.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.AppUserRepository
{
    public class AppUserRepository(UserManager<AppUser> _userManager) : IAppUserRepository
    {
        public async Task<AppUser> GetUserById(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }
    }
}
