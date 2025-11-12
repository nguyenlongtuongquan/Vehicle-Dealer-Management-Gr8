using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.BLL.IService
{
    public interface ISalesDocumentService
    {
        Task<SalesDocument?> GetSalesDocumentWithDetailsAsync(int id);
        Task<IEnumerable<SalesDocument>> GetSalesDocumentsByDealerIdAsync(int dealerId, string? type = null, string? status = null);
        Task<IEnumerable<SalesDocument>> GetSalesDocumentsByCustomerIdAsync(int customerId, string? type = null);
        Task<IEnumerable<SalesDocument>> GetSalesDocumentsByDateRangeAsync(DateTime startDate, DateTime endDate, string? type = null);
        Task<SalesDocument> CreateQuoteAsync(int dealerId, int customerId, int createdBy, int? promotionId = null);
        Task<SalesDocument> ConvertQuoteToOrderAsync(int quoteId);
        Task<SalesDocument> ConvertOrderToContractAsync(int orderId);
        Task<SalesDocument> UpdateSalesDocumentStatusAsync(int id, string status);
        Task<bool> SalesDocumentExistsAsync(int id);
        Task<bool> HasSalesDocumentLinesAsync(int vehicleId);
    }
}

