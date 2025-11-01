using Vehicle_Dealer_Management.DAL.Models;
using Vehicle_Dealer_Management.DAL.Repositories;

namespace Vehicle_Dealer_Management.BLL.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _customerRepository.GetAllAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _customerRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllCustomersAsync();
            }

            return await _customerRepository.SearchCustomersAsync(searchTerm);
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            // Business logic: Validate customer data
            if (string.IsNullOrWhiteSpace(customer.FullName))
            {
                throw new ArgumentException("Customer full name is required", nameof(customer));
            }

            if (string.IsNullOrWhiteSpace(customer.PhoneNumber) && string.IsNullOrWhiteSpace(customer.Email))
            {
                throw new ArgumentException("Either phone number or email is required", nameof(customer));
            }

            customer.CreatedDate = DateTime.Now;

            return await _customerRepository.AddAsync(customer);
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            if (!await _customerRepository.ExistsAsync(customer.Id))
            {
                throw new KeyNotFoundException($"Customer with ID {customer.Id} not found");
            }

            customer.UpdatedDate = DateTime.Now;

            await _customerRepository.UpdateAsync(customer);
        }

        public async Task DeleteCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with ID {id} not found");
            }

            // Business logic: Check if customer has sales
            var customerWithSales = await _customerRepository.GetCustomerWithSalesAsync(id);
            if (customerWithSales?.Sales != null && customerWithSales.Sales.Any())
            {
                throw new InvalidOperationException("Cannot delete customer that has associated sales");
            }

            await _customerRepository.DeleteAsync(id);
        }

        public async Task<bool> CustomerExistsAsync(int id)
        {
            return await _customerRepository.ExistsAsync(id);
        }
    }
}

