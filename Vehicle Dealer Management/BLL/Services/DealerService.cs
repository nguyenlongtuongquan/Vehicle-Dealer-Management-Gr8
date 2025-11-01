using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.DAL.Repositories;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public class DealerService : IDealerService
    {
        private readonly IDealerRepository _dealerRepository;

        public DealerService(IDealerRepository dealerRepository)
        {
            _dealerRepository = dealerRepository;
        }

        public async Task<IEnumerable<Dealer>> GetAllDealersAsync()
        {
            return await _dealerRepository.GetAllAsync();
        }

        public async Task<Dealer?> GetDealerByIdAsync(int id)
        {
            return await _dealerRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Dealer>> GetActiveDealersAsync()
        {
            return await _dealerRepository.GetActiveDealersAsync();
        }

        public async Task<Dealer> CreateDealerAsync(Dealer dealer)
        {
            if (dealer == null)
            {
                throw new ArgumentNullException(nameof(dealer));
            }

            // Business logic: Validate dealer data
            if (string.IsNullOrWhiteSpace(dealer.Name))
            {
                throw new ArgumentException("Dealer name is required", nameof(dealer));
            }

            dealer.CreatedDate = DateTime.Now;
            dealer.IsActive = true;

            return await _dealerRepository.AddAsync(dealer);
        }

        public async Task UpdateDealerAsync(Dealer dealer)
        {
            if (dealer == null)
            {
                throw new ArgumentNullException(nameof(dealer));
            }

            if (!await _dealerRepository.ExistsAsync(dealer.Id))
            {
                throw new KeyNotFoundException($"Dealer with ID {dealer.Id} not found");
            }

            dealer.UpdatedDate = DateTime.Now;

            await _dealerRepository.UpdateAsync(dealer);
        }

        public async Task DeleteDealerAsync(int id)
        {
            var dealer = await _dealerRepository.GetByIdAsync(id);
            if (dealer == null)
            {
                throw new KeyNotFoundException($"Dealer with ID {id} not found");
            }

            await _dealerRepository.DeleteAsync(id);
        }

        public async Task<bool> DealerExistsAsync(int id)
        {
            return await _dealerRepository.ExistsAsync(id);
        }
    }
}

