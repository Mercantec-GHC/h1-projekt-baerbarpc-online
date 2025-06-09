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
        Task UpdatePasswordAsync(int userId, string newPassword); // Renamed parameter
    }
}
