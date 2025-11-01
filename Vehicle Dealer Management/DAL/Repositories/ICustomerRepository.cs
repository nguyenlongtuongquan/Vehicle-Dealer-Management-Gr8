using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.Repositories
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
        Task<Customer?> GetCustomerWithSalesAsync(int id);
    }
}

