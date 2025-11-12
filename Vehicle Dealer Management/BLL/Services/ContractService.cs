using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Constants;
using Vehicle_Dealer_Management.DAL.IRepository;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public class ContractService : IContractService
    {
        private readonly ISalesContractRepository _contractRepository;
        private readonly ISalesDocumentRepository _salesDocumentRepository;

        public ContractService(
            ISalesContractRepository contractRepository,
            ISalesDocumentRepository salesDocumentRepository)
        {
            _contractRepository = contractRepository;
            _salesDocumentRepository = salesDocumentRepository;
        }

        public async Task<SalesContract?> GetContractByIdAsync(int id)
        {
            return await _contractRepository.GetWithDetailsAsync(id);
        }

        public async Task<SalesContract?> GetContractByQuoteIdAsync(int quoteId)
        {
            return await _contractRepository.GetByQuoteIdAsync(quoteId);
        }

        public async Task<SalesContract?> GetContractByOrderIdAsync(int orderId)
        {
            return await _contractRepository.GetByOrderIdAsync(orderId);
        }

        public async Task<IEnumerable<SalesContract>> GetContractsForDealerAsync(int dealerId)
        {
            return await _contractRepository.GetContractsForDealerAsync(dealerId);
        }

        public async Task<IEnumerable<SalesContract>> GetContractsForCustomerAsync(int customerId)
        {
            return await _contractRepository.GetContractsForCustomerAsync(customerId);
        }

        public async Task<SalesContract> CreateContractAsync(int quoteId, int dealerUserId)
        {
            var existing = await _contractRepository.GetByQuoteIdAsync(quoteId);
            if (existing != null)
            {
                throw new InvalidOperationException("Báo giá này đã có hợp đồng.");
            }

            var quote = await _salesDocumentRepository.GetSalesDocumentWithDetailsAsync(quoteId);
            if (quote == null || quote.Type != "QUOTE")
            {
                throw new KeyNotFoundException("Không tìm thấy báo giá hợp lệ.");
            }

            if (quote.Status != "ACCEPTED" && quote.Status != "CONVERTED")
            {
                throw new InvalidOperationException("Chỉ tạo hợp đồng khi báo giá đã được khách hàng chấp nhận.");
            }

            var contract = new SalesContract
            {
                QuoteId = quote.Id,
                DealerId = quote.DealerId,
                CustomerId = quote.CustomerId,
                CreatedBy = dealerUserId,
                Status = SalesContractStatus.PendingCustomerSignature,
                CreatedAt = DateTime.UtcNow
            };

            return await _contractRepository.AddAsync(contract);
        }

        public async Task<SalesContract> SaveCustomerSignatureAsync(int contractId, string signaturePath)
        {
            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                throw new KeyNotFoundException("Không tìm thấy hợp đồng.");
            }

            if (contract.Status == SalesContractStatus.Cancelled)
            {
                throw new InvalidOperationException("Hợp đồng đã bị hủy.");
            }

            contract.CustomerSignatureUrl = signaturePath;
            contract.CustomerSignedAt = DateTime.UtcNow;
            contract.Status = SalesContractStatus.CustomerSigned;
            contract.UpdatedAt = DateTime.UtcNow;

            await _contractRepository.UpdateAsync(contract);
            return contract;
        }

        public async Task<SalesContract> MarkContractLinkedToOrderAsync(int contractId, int orderId)
        {
            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                throw new KeyNotFoundException("Không tìm thấy hợp đồng.");
            }

            contract.OrderId = orderId;
            contract.Status = SalesContractStatus.OrderCreated;
            contract.UpdatedAt = DateTime.UtcNow;

            await _contractRepository.UpdateAsync(contract);
            return contract;
        }
    }
}


