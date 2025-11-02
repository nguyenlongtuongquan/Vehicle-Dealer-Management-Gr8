using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.DAL.IRepository;
using Vehicle_Dealer_Management.BLL.IService;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public class TestDriveService : ITestDriveService
    {
        private readonly ITestDriveRepository _testDriveRepository;

        public TestDriveService(ITestDriveRepository testDriveRepository)
        {
            _testDriveRepository = testDriveRepository;
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByCustomerIdAsync(int customerId)
        {
            return await _testDriveRepository.GetTestDrivesByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByDealerIdAsync(int dealerId)
        {
            return await _testDriveRepository.GetTestDrivesByDealerIdAsync(dealerId);
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByDealerAndDateAsync(int dealerId, DateTime date)
        {
            return await _testDriveRepository.GetTestDrivesByDealerAndDateAsync(dealerId, date);
        }

        public async Task<IEnumerable<TestDrive>> GetTestDrivesByStatusAsync(string status, int? dealerId = null)
        {
            return await _testDriveRepository.GetTestDrivesByStatusAsync(status, dealerId);
        }

        public async Task<TestDrive?> GetTestDriveByIdAsync(int id)
        {
            return await _testDriveRepository.GetByIdAsync(id);
        }

        public async Task<TestDrive> CreateTestDriveAsync(TestDrive testDrive)
        {
            if (testDrive == null)
            {
                throw new ArgumentNullException(nameof(testDrive));
            }

            // Business logic: Validate test drive
            if (testDrive.ScheduleTime < DateTime.UtcNow)
            {
                throw new ArgumentException("Schedule time must be in the future", nameof(testDrive));
            }

            if (string.IsNullOrWhiteSpace(testDrive.Status))
            {
                testDrive.Status = "REQUESTED";
            }

            testDrive.CreatedAt = DateTime.UtcNow;

            return await _testDriveRepository.AddAsync(testDrive);
        }

        public async Task UpdateTestDriveAsync(TestDrive testDrive)
        {
            if (testDrive == null)
            {
                throw new ArgumentNullException(nameof(testDrive));
            }

            // Business logic: Validate test drive
            if (testDrive.ScheduleTime < DateTime.UtcNow)
            {
                throw new ArgumentException("Schedule time must be in the future", nameof(testDrive));
            }

            testDrive.UpdatedAt = DateTime.UtcNow;

            await _testDriveRepository.UpdateAsync(testDrive);
        }

        public async Task UpdateTestDriveStatusAsync(int id, string status)
        {
            var testDrive = await _testDriveRepository.GetByIdAsync(id);
            if (testDrive == null)
            {
                throw new KeyNotFoundException($"TestDrive with ID {id} not found");
            }

            testDrive.Status = status;
            testDrive.UpdatedAt = DateTime.UtcNow;

            await _testDriveRepository.UpdateAsync(testDrive);
        }

        public async Task<bool> TestDriveExistsAsync(int id)
        {
            return await _testDriveRepository.ExistsAsync(id);
        }
    }
}

