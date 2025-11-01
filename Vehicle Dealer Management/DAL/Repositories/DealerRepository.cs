using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.Repositories
{
    public class DealerRepository : Repository<Dealer>, IDealerRepository
    {
        public DealerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Dealer>> GetActiveDealersAsync()
        {
            return await FindAsync(d => d.IsActive == true);
        }
    }
}

