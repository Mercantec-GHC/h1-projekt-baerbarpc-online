using System.Threading.Tasks;
using BlazorMarkedsplads.Models;

namespace BlazorMarkedsplads.Services
{
    public interface IUserRepository
    {
        Task<int> InsertAsync(User user);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task UpdateAsync(User user);
    }
}
