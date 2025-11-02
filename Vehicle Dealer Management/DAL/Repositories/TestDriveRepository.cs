using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.IRepository;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.Repositories
{
    public class TestDriveRepository : Repository<TestDrive>, ITestDriveRepository
    {
        public TestDriveRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByCustomerIdAsync(int customerId)
        {
            return await _context.TestDrives
                .Where(t => t.CustomerId == customerId)
                .Include(t => t.Vehicle)
                .Include(t => t.Dealer)
                .OrderByDescending(t => t.ScheduleTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByDealerIdAsync(int dealerId)
        {
            return await _context.TestDrives
                .Where(t => t.DealerId == dealerId)
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Include(t => t.Dealer)
                .OrderBy(t => t.ScheduleTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByDealerAndDateAsync(int dealerId, DateTime date)
        {
            return await _context.TestDrives
                .Where(t => t.DealerId == dealerId && t.ScheduleTime.Date == date.Date)
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Include(t => t.Dealer)
                .OrderBy(t => t.ScheduleTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByStatusAsync(string status, int? dealerId = null)
        {
            var query = _context.TestDrives.Where(t => t.Status == status);
            
            if (dealerId.HasValue)
            {
                query = query.Where(t => t.DealerId == dealerId.Value);
            }

            return await query
                .Include(t => t.Customer)
                .Include(t => t.Vehicle)
                .Include(t => t.Dealer)
                .OrderBy(t => t.ScheduleTime)
                .ToListAsync();
        }
    }
}

