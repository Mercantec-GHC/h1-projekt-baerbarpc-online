// Services/IListingRepository.cs
using System.Threading.Tasks;
using BlazorMarkedplads.Models;
using BlazorMarkedsplads.Models;          //  ←  vigtigt, ellers kender interfacet ikke Listing

namespace BlazorMarkedsplads.Services     //  SAME namespace i ALLE service-filer
{
    public interface IListingRepository
    {
        Task<int> InsertAsync(Listing listing);
    }
}
