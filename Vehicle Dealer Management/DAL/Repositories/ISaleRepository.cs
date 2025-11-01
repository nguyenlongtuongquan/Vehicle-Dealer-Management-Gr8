using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.Repositories
{
    public interface ISaleRepository : IRepository<Sale>
    {
        Task<IEnumerable<Sale>> GetSalesByCustomerIdAsync(int customerId);
        Task<IEnumerable<Sale>> GetSalesByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Sale?> GetSaleWithDetailsAsync(int id);
    }
}

