using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public interface IDealerService
    {
        Task<IEnumerable<Dealer>> GetAllDealersAsync();
        Task<Dealer?> GetDealerByIdAsync(int id);
        Task<IEnumerable<Dealer>> GetActiveDealersAsync();
        Task<Dealer> CreateDealerAsync(Dealer dealer);
        Task UpdateDealerAsync(Dealer dealer);
        Task DeleteDealerAsync(int id);
        Task<bool> DealerExistsAsync(int id);
    }
}

