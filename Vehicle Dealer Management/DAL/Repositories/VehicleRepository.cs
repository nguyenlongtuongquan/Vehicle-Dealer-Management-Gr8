using Microsoft.EntityFrameworkCore;
using Vehicle_Dealer_Management.DAL.Data;
using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.DAL.Repositories
{
    public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
        {
            return await _dbSet
                .Where(v => v.IsAvailable == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vehicle>> SearchVehiclesAsync(string searchTerm)
        {
            return await _dbSet
                .Where(v => v.Make.Contains(searchTerm) ||
                           v.Model.Contains(searchTerm) ||
                           (v.VinNumber != null && v.VinNumber.Contains(searchTerm)) ||
                           (v.LicensePlate != null && v.LicensePlate.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleWithSalesAsync(int id)
        {
            return await _dbSet
                .Include(v => v.Sales)
                .FirstOrDefaultAsync(v => v.Id == id);
        }
    }
}

