using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SMS.Enums;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO;

namespace SMS.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IRepository _dbContext;

        public CustomerService(IRepository dbContext)
        {
            _dbContext = dbContext;
        }

        public IList<Customer> GetAllCustomers(IDateTimeRange dateTimeRange)
        {
            var startTime = dateTimeRange.From;
            var endTime = dateTimeRange.To;

            return _dbContext.Get<Customer>(c => c.DeletedAt == null && c.CreatedAt <= endTime && c.CreatedAt >= startTime).ToList();
        }

        public Customer GetCustomerById(int customerId)
        {
            return _dbContext.Get<Customer>(c => c.CustomerId == customerId && c.DeletedAt == null)
                             .Include(c => c.Transactions)
                             .FirstOrDefault();
        }

        public IList<Customer> GetCustomersByIds(IEnumerable<int> customerIds)
        {
            return _dbContext.Get<Customer>(c => customerIds.Contains(c.CustomerId) && c.DeletedAt == null)
                             .Include(c => c.Transactions)
                             .ToList();
        }

        public void CreateCustomer(Customer customer)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    _dbContext.Create<Customer>(customer);
                    _dbContext.Save();
                    _dbContext.CommitTransaction();
                }
                catch (Exception)
                {
                    _dbContext.RollbackTransaction();
                    throw;
                }
            }
        }

        public void UpdateCustomer(Customer customer)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    customer.UpdatedAt = DateTime.Now;
                    _dbContext.Update<Customer>(customer);
                    _dbContext.Save();
                    _dbContext.CommitTransaction();
                }
                catch (Exception)
                {
                    _dbContext.RollbackTransaction();
                    throw;
                }
            }
        }

        public void DeleteCustomer(int customerId)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    var customer = _dbContext.GetById<Customer>(customerId);
                    if (customer != null)
                    {
                        customer.DeletedAt = DateTime.Now;
                        _dbContext.Update<Customer>(customer);
                        _dbContext.Save();
                    }
                    _dbContext.CommitTransaction();
                }
                catch (Exception)
                {
                    _dbContext.RollbackTransaction();
                    throw;
                }
            }
        }

        public void DeleteCustomers(IEnumerable<int> customerIds)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    var customers = _dbContext.Get<Customer>(c => customerIds.Contains(c.CustomerId) && c.DeletedAt == null).ToList();
                    foreach (var customer in customers)
                    {
                        customer.DeletedAt = DateTime.Now;
                        _dbContext.Update<Customer>(customer);
                    }
                    _dbContext.Save();
                    _dbContext.CommitTransaction();
                }
                catch (Exception)
                {
                    _dbContext.RollbackTransaction();
                    throw;
                }
            }
        }

        public Customer? GetCustomerByNIC(string customerNIC)
        {
            return _dbContext.Get<Customer>(c => c.CustomerNIC == customerNIC && c.DeletedAt == null)
                             .Include(c => c.Transactions)
                             .FirstOrDefault();
        }

        public int? GetCustomerCount()
        {
            return _dbContext.Get<Customer>(c => c.DeletedAt == null).Count();
        }
    }
}
