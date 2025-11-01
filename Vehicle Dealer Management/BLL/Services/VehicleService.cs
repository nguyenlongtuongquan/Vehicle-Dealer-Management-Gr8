using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.DAL.Repositories;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;

        public VehicleService(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            return await _vehicleRepository.GetAllAsync();
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int id)
        {
            return await _vehicleRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
        {
            return await _vehicleRepository.GetAvailableVehiclesAsync();
        }

        public async Task<IEnumerable<Vehicle>> SearchVehiclesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllVehiclesAsync();
            }

            return await _vehicleRepository.SearchVehiclesAsync(searchTerm);
        }

        public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle));
            }

            // Business logic: Validate vehicle data
            if (string.IsNullOrWhiteSpace(vehicle.Make))
            {
                throw new ArgumentException("Vehicle make is required", nameof(vehicle));
            }

            if (string.IsNullOrWhiteSpace(vehicle.Model))
            {
                throw new ArgumentException("Vehicle model is required", nameof(vehicle));
            }

            if (vehicle.Year < 1900 || vehicle.Year > DateTime.Now.Year + 1)
            {
                throw new ArgumentException("Invalid year", nameof(vehicle));
            }

            if (vehicle.Price <= 0)
            {
                throw new ArgumentException("Price must be greater than 0", nameof(vehicle));
            }

            vehicle.CreatedDate = DateTime.Now;
            vehicle.IsAvailable = true;

            return await _vehicleRepository.AddAsync(vehicle);
        }

        public async Task UpdateVehicleAsync(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle));
            }

            if (!await _vehicleRepository.ExistsAsync(vehicle.Id))
            {
                throw new KeyNotFoundException($"Vehicle with ID {vehicle.Id} not found");
            }

            vehicle.UpdatedDate = DateTime.Now;

            await _vehicleRepository.UpdateAsync(vehicle);
        }

        public async Task DeleteVehicleAsync(int id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null)
            {
                throw new KeyNotFoundException($"Vehicle with ID {id} not found");
            }

            // Business logic: Check if vehicle has sales
            var vehicleWithSales = await _vehicleRepository.GetVehicleWithSalesAsync(id);
            if (vehicleWithSales?.Sales != null && vehicleWithSales.Sales.Any())
            {
                throw new InvalidOperationException("Cannot delete vehicle that has associated sales");
            }

            await _vehicleRepository.DeleteAsync(id);
        }

        public async Task<bool> VehicleExistsAsync(int id)
        {
            return await _vehicleRepository.ExistsAsync(id);
        }
    }
}

