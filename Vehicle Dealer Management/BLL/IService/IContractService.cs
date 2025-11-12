using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.BLL.IService
{
    public interface IContractService
    {
        Task<SalesContract?> GetContractByIdAsync(int id);
        Task<SalesContract?> GetContractByQuoteIdAsync(int quoteId);
        Task<SalesContract?> GetContractByOrderIdAsync(int orderId);
        Task<IEnumerable<SalesContract>> GetContractsForDealerAsync(int dealerId);
        Task<IEnumerable<SalesContract>> GetContractsForCustomerAsync(int customerId);
        Task<SalesContract> CreateContractAsync(int quoteId, int dealerUserId);
        Task<SalesContract> SaveCustomerSignatureAsync(int contractId, string signaturePath);
        Task<SalesContract> MarkContractLinkedToOrderAsync(int contractId, int orderId);
    }
}


