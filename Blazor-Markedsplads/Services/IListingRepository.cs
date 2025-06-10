using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorMarkedsplads.Models;

namespace BlazorMarkedsplads.Services
{
    public interface IListingRepository
    {
        Task<int> InsertAsync(Listing listing);
        Task<List<Listing>> GetPremiumAsync(int take = 4);
        Task<List<Listing>> SearchAsync(string term);
        Task<List<Listing>> GetByUserIdAsync(int userId);
        Task<Listing?> GetByIdAsync(int id);

        Task AddImageAsync(int listingId, string imagePath);
    }
}
