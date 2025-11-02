using Vehicle_Dealer_Management.DAL.Models;

namespace Vehicle_Dealer_Management.BLL.IService
{
    public interface ITestDriveService
    {
        Task<IEnumerable<TestDrive>> GetTestDrivesByCustomerIdAsync(int customerId);
        Task<IEnumerable<TestDrive>> GetTestDrivesByDealerIdAsync(int dealerId);
        Task<IEnumerable<TestDrive>> GetTestDrivesByDealerAndDateAsync(int dealerId, DateTime date);
        Task<IEnumerable<TestDrive>> GetTestDrivesByStatusAsync(string status, int? dealerId = null);
        Task<TestDrive?> GetTestDriveByIdAsync(int id);
        Task<TestDrive> CreateTestDriveAsync(TestDrive testDrive);
        Task UpdateTestDriveAsync(TestDrive testDrive);
        Task UpdateTestDriveStatusAsync(int id, string status);
        Task<bool> TestDriveExistsAsync(int id);
    }
}

