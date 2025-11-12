using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.DAL.IRepository;
using Vehicle_Dealer_Management.BLL.IService;
using Vehicle_Dealer_Management.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Constants;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ISalesDocumentRepository _salesDocumentRepository;
        private readonly IContractService _contractService;
        private readonly ApplicationDbContext _context;

        public PaymentService(
            IPaymentRepository paymentRepository,
            ISalesDocumentRepository salesDocumentRepository,
            IContractService contractService,
            ApplicationDbContext context)
        {
            _paymentRepository = paymentRepository;
            _salesDocumentRepository = salesDocumentRepository;
            _contractService = contractService;
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetPaymentsBySalesDocumentIdAsync(int salesDocumentId)
        {
            return await _paymentRepository.GetPaymentsBySalesDocumentIdAsync(salesDocumentId);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsBySalesDocumentIdsAsync(IEnumerable<int> salesDocumentIds)
        {
            return await _paymentRepository.GetPaymentsBySalesDocumentIdsAsync(salesDocumentIds);
        }

        public async Task<decimal> GetTotalPaidAmountAsync(int salesDocumentId)
        {
            return await _paymentRepository.GetTotalPaidAmountAsync(salesDocumentId);
        }

        public async Task<Payment> CreatePaymentAsync(int salesDocumentId, string method, decimal amount, string? metaJson = null)
        {
            // Validate sales document exists
            var salesDocument = await _salesDocumentRepository.GetByIdAsync(salesDocumentId);
            if (salesDocument == null)
            {
                throw new KeyNotFoundException($"SalesDocument with ID {salesDocumentId} not found");
            }

            // Validate method
            if (method != "CASH" && method != "FINANCE" && method != "MOMO" && method != "VNPAY")
            {
                throw new ArgumentException("Payment method must be 'CASH', 'FINANCE', 'MOMO', or 'VNPAY'", nameof(method));
            }

            // Validate amount
            if (amount <= 0)
            {
                throw new ArgumentException("Payment amount must be greater than 0", nameof(amount));
            }

            if (salesDocument.Type == "ORDER")
            {
                var contract = await _contractService.GetContractByOrderIdAsync(salesDocumentId);
                if (contract == null || !SalesContractStatus.IsSigned(contract.Status))
                {
                    throw new InvalidOperationException("Không thể tạo thanh toán khi đơn hàng chưa có hợp đồng được ký bởi khách hàng.");
                }
            }

            var payment = new Payment
            {
                SalesDocumentId = salesDocumentId,
                Method = method,
                Amount = amount,
                MetaJson = metaJson,
                PaidAt = DateTime.UtcNow
            };

            var createdPayment = await _paymentRepository.AddAsync(payment);

            // Auto-update sales document status if fully paid
            await UpdateSalesDocumentStatusIfPaidAsync(salesDocumentId);

            return createdPayment;
        }

        private async Task UpdateSalesDocumentStatusIfPaidAsync(int salesDocumentId)
        {
            var salesDocument = await _salesDocumentRepository.GetSalesDocumentWithDetailsAsync(salesDocumentId);
            if (salesDocument == null || salesDocument.Lines == null) return;

            // Calculate total amount from lines
            var totalAmount = salesDocument.Lines.Sum(l => l.UnitPrice * l.Qty - l.DiscountValue);

            // Get total paid amount
            var totalPaid = await GetTotalPaidAmountAsync(salesDocumentId);

            // Update status based on payment
            if (totalPaid >= totalAmount && salesDocument.Status != "PAID" && salesDocument.Type == "ORDER")
            {
                salesDocument.Status = "PAID";
                salesDocument.UpdatedAt = DateTime.UtcNow;
                await _salesDocumentRepository.UpdateAsync(salesDocument);
            }
            else if (totalPaid > 0 && salesDocument.Status == "CONFIRMED" && salesDocument.Type == "ORDER")
            {
                salesDocument.Status = "PARTIAL_PAID";
                salesDocument.UpdatedAt = DateTime.UtcNow;
                await _salesDocumentRepository.UpdateAsync(salesDocument);
            }
        }

        public async Task<bool> PaymentExistsAsync(int id)
        {
            return await _paymentRepository.ExistsAsync(id);
        }
    }
}

