using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            return await _dbSet
                .Where(c => c.FullName.Contains(searchTerm) ||
                           (c.PhoneNumber != null && c.PhoneNumber.Contains(searchTerm)) ||
                           (c.Email != null && c.Email.Contains(searchTerm)) ||
                           (c.IdCardNumber != null && c.IdCardNumber.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerWithSalesAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Sales)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}

