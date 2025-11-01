using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.Repositories
{
    public class SaleRepository : Repository<Sale>, ISaleRepository
    {
        public SaleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Sale>> GetSalesByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(s => s.CustomerId == customerId)
                .Include(s => s.Vehicle)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Sale>> GetSalesByVehicleIdAsync(int vehicleId)
        {
            return await _dbSet
                .Where(s => s.VehicleId == vehicleId)
                .Include(s => s.Customer)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
                .Include(s => s.Vehicle)
                .Include(s => s.Customer)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<Sale?> GetSaleWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(s => s.Vehicle)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}

