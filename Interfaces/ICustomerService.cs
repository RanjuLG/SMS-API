using SMS.Models;
using SMS.Models.DTO;
using System.Collections.Generic;

namespace SMS.Interfaces
{
    public interface ICustomerService
    {
        IList<Customer> GetAllCustomers();
        Customer GetCustomerById(int customerId);
        IList<Customer> GetCustomersByIds(IEnumerable<int> customerIds);
        void CreateCustomer(Customer customer);
        void UpdateCustomer(Customer customer);
        void DeleteCustomer(int customerId);
        void DeleteCustomers(IEnumerable<int> customerIds);
        Customer? GetCustomerByNIC(string customerNIC);
        //Customer? GetCustomerByNIC_(string customerNIC);
    }
}
