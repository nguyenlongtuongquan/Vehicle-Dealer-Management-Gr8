using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.DAL.Repositories;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ICustomerRepository _customerRepository;

        public SaleService(
            ISaleRepository saleRepository,
            IVehicleRepository vehicleRepository,
            ICustomerRepository customerRepository)
        {
            _saleRepository = saleRepository;
            _vehicleRepository = vehicleRepository;
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<Sale>> GetAllSalesAsync()
        {
            return await _saleRepository.GetAllAsync();
        }

        public async Task<Sale?> GetSaleByIdAsync(int id)
        {
            return await _saleRepository.GetSaleWithDetailsAsync(id);
        }

        public async Task<IEnumerable<Sale>> GetSalesByCustomerIdAsync(int customerId)
        {
            return await _saleRepository.GetSalesByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<Sale>> GetSalesByVehicleIdAsync(int vehicleId)
        {
            return await _saleRepository.GetSalesByVehicleIdAsync(vehicleId);
        }

        public async Task<IEnumerable<Sale>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _saleRepository.GetSalesByDateRangeAsync(startDate, endDate);
        }

        public async Task<Sale> CreateSaleAsync(Sale sale)
        {
            if (sale == null)
            {
                throw new ArgumentNullException(nameof(sale));
            }

            // Business logic: Validate sale data
            var vehicle = await _vehicleRepository.GetByIdAsync(sale.VehicleId);
            if (vehicle == null)
            {
                throw new KeyNotFoundException($"Vehicle with ID {sale.VehicleId} not found");
            }

            if (!vehicle.IsAvailable)
            {
                throw new InvalidOperationException("Vehicle is not available for sale");
            }

            var customer = await _customerRepository.GetByIdAsync(sale.CustomerId);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {sale.CustomerId} not found");
            }

            if (sale.SalePrice <= 0)
            {
                throw new ArgumentException("Sale price must be greater than 0", nameof(sale));
            }

            // Business logic: Mark vehicle as unavailable after sale
            vehicle.IsAvailable = false;
            vehicle.UpdatedDate = DateTime.Now;
            await _vehicleRepository.UpdateAsync(vehicle);

            sale.SaleDate = DateTime.Now;
            sale.CreatedDate = DateTime.Now;

            return await _saleRepository.AddAsync(sale);
        }

        public async Task UpdateSaleAsync(Sale sale)
        {
            if (sale == null)
            {
                throw new ArgumentNullException(nameof(sale));
            }

            if (!await _saleRepository.ExistsAsync(sale.Id))
            {
                throw new KeyNotFoundException($"Sale with ID {sale.Id} not found");
            }

            await _saleRepository.UpdateAsync(sale);
        }

        public async Task DeleteSaleAsync(int id)
        {
            var sale = await _saleRepository.GetByIdAsync(id);
            if (sale == null)
            {
                throw new KeyNotFoundException($"Sale with ID {id} not found");
            }

            // Business logic: Mark vehicle as available again if sale is deleted
            var vehicle = await _vehicleRepository.GetByIdAsync(sale.VehicleId);
            if (vehicle != null)
            {
                vehicle.IsAvailable = true;
                vehicle.UpdatedDate = DateTime.Now;
                await _vehicleRepository.UpdateAsync(vehicle);
            }

            await _saleRepository.DeleteAsync(id);
        }

        public async Task<bool> SaleExistsAsync(int id)
        {
            return await _saleRepository.ExistsAsync(id);
        }
    }
}

