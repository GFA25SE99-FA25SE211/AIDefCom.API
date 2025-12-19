using AIDefCom.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Repositories.MemberNoteRepository
{
    public interface IMemberNoteRepository
    {
        Task<IEnumerable<MemberNote>> GetAllAsync();
        Task<MemberNote?> GetByIdAsync(int id);
        Task<IEnumerable<MemberNote>> GetBySessionIdAsync(int sessionId);
        Task<IEnumerable<MemberNote>> GetByUserIdAsync(string userId);
        Task AddAsync(MemberNote note);
        Task UpdateAsync(MemberNote note);
        Task DeleteAsync(int id);
    }
}
