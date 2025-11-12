using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.IRepository;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.Repositories
{
    public class SalesContractRepository : Repository<SalesContract>, ISalesContractRepository
    {
        private readonly ApplicationDbContext _context;

        public SalesContractRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<SalesContract?> GetByQuoteIdAsync(int quoteId)
        {
            return await _context.SalesContracts
                .Include(c => c.Quote)
                    .ThenInclude(q => q.Lines)
                .Include(c => c.Quote)
                    .ThenInclude(q => q.Dealer)
                .Include(c => c.Quote)
                    .ThenInclude(q => q.Customer)
                .Include(c => c.Customer)
                .Include(c => c.Dealer)
                .Include(c => c.Order)
                    .ThenInclude(o => o.Payments)
                .FirstOrDefaultAsync(c => c.QuoteId == quoteId);
        }

        public async Task<SalesContract?> GetByOrderIdAsync(int orderId)
        {
            return await _context.SalesContracts
                .Include(c => c.Quote)
                .Include(c => c.Order)
                    .ThenInclude(o => o.Lines)
                .Include(c => c.Customer)
                .Include(c => c.Dealer)
                .FirstOrDefaultAsync(c => c.OrderId == orderId);
        }

        public async Task<SalesContract?> GetWithDetailsAsync(int id)
        {
            return await _context.SalesContracts
                .Include(c => c.Quote)
                    .ThenInclude(q => q.Lines)
                    .ThenInclude(l => l.Vehicle)
                .Include(c => c.Order)
                    .ThenInclude(o => o.Lines)
                    .ThenInclude(l => l.Vehicle)
                .Include(c => c.Dealer)
                .Include(c => c.Customer)
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<SalesContract>> GetContractsForDealerAsync(int dealerId)
        {
            return await _context.SalesContracts
                .Include(c => c.Quote)
                .Include(c => c.Order)
                .Include(c => c.Customer)
                .Where(c => c.DealerId == dealerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SalesContract>> GetContractsForCustomerAsync(int customerId)
        {
            return await _context.SalesContracts
                .Include(c => c.Quote)
                .Include(c => c.Order)
                .Include(c => c.Dealer)
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}


