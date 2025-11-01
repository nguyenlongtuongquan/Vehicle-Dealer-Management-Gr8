using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.Repositories
{
    public interface IDealerRepository : IRepository<Dealer>
    {
        Task<IEnumerable<Dealer>> GetActiveDealersAsync();
    }
}

