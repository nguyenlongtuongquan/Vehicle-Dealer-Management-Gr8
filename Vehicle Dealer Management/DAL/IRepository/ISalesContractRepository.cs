using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.IRepository
{
    public interface ISalesContractRepository : IRepository<SalesContract>
    {
        Task<SalesContract?> GetByQuoteIdAsync(int quoteId);
        Task<SalesContract?> GetByOrderIdAsync(int orderId);
        Task<SalesContract?> GetWithDetailsAsync(int id);
        Task<IEnumerable<SalesContract>> GetContractsForDealerAsync(int dealerId);
        Task<IEnumerable<SalesContract>> GetContractsForCustomerAsync(int customerId);
    }
}


