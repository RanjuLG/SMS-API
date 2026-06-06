using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SMS.Enums;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO;
using Serilog;

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
            try
            {
                var startTime = dateTimeRange.From;
                var endTime = dateTimeRange.To;

                Log.Information("Fetching customers from {StartTime} to {EndTime}", startTime, endTime);
                var customers = _dbContext.Get<Customer>(c => c.DeletedAt == null && c.CreatedAt <= endTime && c.CreatedAt >= startTime).ToList();
                
                Log.Information("Retrieved {Count} customers", customers.Count);
                return customers;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching customers in date range {StartTime} to {EndTime}", 
                    dateTimeRange.From, dateTimeRange.To);
                throw;
            }
        }

        public async Task<IQueryable<Customer>> GetCustomersQueryAsync(CustomerSearchRequest request)
        {
            try
            {
                Log.Information("Searching customers with query: Search={Search}, NIC={NIC}, SortBy={SortBy}", 
                    request.Search, request.CustomerNIC, request.SortBy);
                
                var query = _dbContext.Get<Customer>(c => c.DeletedAt == null);
            
                // Apply date range filter
                if (request.From != default && request.To != default)
                {
                    query = query.Where(c => c.CreatedAt >= request.From && c.CreatedAt <= request.To);
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(request.Search))
                {
                    query = query.Where(c => 
                        (c.CustomerName != null && c.CustomerName.Contains(request.Search)) ||
                        c.CustomerNIC.Contains(request.Search) ||
                        (c.CustomerAddress != null && c.CustomerAddress.Contains(request.Search)) ||
                        (c.CustomerContactNo != null && c.CustomerContactNo.Contains(request.Search))
                    );
                }

                // Apply specific NIC filter
                if (!string.IsNullOrEmpty(request.CustomerNIC))
                {
                    query = query.Where(c => c.CustomerNIC.Contains(request.CustomerNIC));
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    query = request.SortBy.ToLower() switch
                    {
                        "customername" => request.SortOrder?.ToLower() == "desc" 
                            ? query.OrderByDescending(c => c.CustomerName) 
                            : query.OrderBy(c => c.CustomerName),
                        "customernic" => request.SortOrder?.ToLower() == "desc" 
                            ? query.OrderByDescending(c => c.CustomerNIC) 
                            : query.OrderBy(c => c.CustomerNIC),
                        "createdat" => request.SortOrder?.ToLower() == "desc" 
                            ? query.OrderByDescending(c => c.CreatedAt) 
                            : query.OrderBy(c => c.CreatedAt),
                        _ => query.OrderBy(c => c.CustomerName)
                    };
                }
                else
                {
                    // Default sorting
                    query = query.OrderBy(c => c.CustomerName);
                }

                return await Task.FromResult(query);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error building customer query");
                throw;
            }
        }

        public Customer GetCustomerById(int customerId)
        {
            try
            {
                Log.Debug("Fetching customer with ID: {CustomerId}", customerId);
                var customer = _dbContext.Get<Customer>(c => c.CustomerId == customerId && c.DeletedAt == null)
                                         .Include(c => c.Transactions)
                                         .FirstOrDefault();
                
                if (customer == null)
                {
                    Log.Warning("Customer not found with ID: {CustomerId}", customerId);
                }
                else
                {
                    Log.Debug("Found customer: {CustomerName} (ID: {CustomerId})", customer.CustomerName, customerId);
                }
                
                return customer;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching customer with ID: {CustomerId}", customerId);
                throw;
            }
        }

        public IList<Customer> GetCustomersByIds(IEnumerable<int> customerIds)
        {
            try
            {
                var ids = customerIds.ToList();
                Log.Information("Fetching {Count} customers by IDs", ids.Count);
                
                var customers = _dbContext.Get<Customer>(c => customerIds.Contains(c.CustomerId) && c.DeletedAt == null)
                                         .Include(c => c.Transactions)
                                         .ToList();
                
                Log.Information("Retrieved {Count} of {Requested} customers", customers.Count, ids.Count);
                return customers;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching customers by IDs");
                throw;
            }
        }

        public void CreateCustomer(Customer customer)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    Log.Information("Creating new customer: {CustomerName}, NIC: {CustomerNIC}", 
                        customer.CustomerName, customer.CustomerNIC);
                    
                    _dbContext.Create<Customer>(customer);
                    _dbContext.Save();
                    _dbContext.CommitTransaction();
                    
                    Log.Information("Customer created successfully with ID: {CustomerId}", customer.CustomerId);
                }
                catch (Exception ex)
                {
                    _dbContext.RollbackTransaction();
                    Log.Error(ex, "Error creating customer: {CustomerName}, NIC: {CustomerNIC}", 
                        customer.CustomerName, customer.CustomerNIC);
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
                    Log.Information("Updating customer ID: {CustomerId}, Name: {CustomerName}", 
                        customer.CustomerId, customer.CustomerName);
                    
                    customer.UpdatedAt = DateTime.Now;
                    _dbContext.Update<Customer>(customer);
                    _dbContext.Save();
                    _dbContext.CommitTransaction();
                    
                    Log.Information("Customer {CustomerId} updated successfully", customer.CustomerId);
                }
                catch (Exception ex)
                {
                    _dbContext.RollbackTransaction();
                    Log.Error(ex, "Error updating customer ID: {CustomerId}", customer.CustomerId);
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
                    Log.Information("Soft deleting customer ID: {CustomerId}", customerId);
                    
                    var customer = _dbContext.GetById<Customer>(customerId);
                    if (customer != null)
                    {
                        customer.DeletedAt = DateTime.Now;
                        _dbContext.Update<Customer>(customer);
                        _dbContext.Save();
                        Log.Information("Customer {CustomerName} (ID: {CustomerId}) soft deleted successfully", 
                            customer.CustomerName, customerId);
                    }
                    else
                    {
                        Log.Warning("Attempted to delete non-existent customer ID: {CustomerId}", customerId);
                    }
                    
                    _dbContext.CommitTransaction();
                }
                catch (Exception ex)
                {
                    _dbContext.RollbackTransaction();
                    Log.Error(ex, "Error deleting customer ID: {CustomerId}", customerId);
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
                    var ids = customerIds.ToList();
                    Log.Information("Soft deleting {Count} customers", ids.Count);
                    
                    var customers = _dbContext.Get<Customer>(c => customerIds.Contains(c.CustomerId) && c.DeletedAt == null).ToList();
                    
                    foreach (var customer in customers)
                    {
                        customer.DeletedAt = DateTime.Now;
                        _dbContext.Update<Customer>(customer);
                        Log.Debug("Marking customer {CustomerName} (ID: {CustomerId}) as deleted", 
                            customer.CustomerName, customer.CustomerId);
                    }
                    
                    _dbContext.Save();
                    _dbContext.CommitTransaction();
                    
                    Log.Information("Successfully soft deleted {DeletedCount} of {RequestedCount} customers", 
                        customers.Count, ids.Count);
                }
                catch (Exception ex)
                {
                    _dbContext.RollbackTransaction();
                    Log.Error(ex, "Error deleting multiple customers");
                    throw;
                }
            }
        }

        public Customer? GetCustomerByNIC(string customerNIC)
        {
            try
            {
                Log.Debug("Fetching customer with NIC: {CustomerNIC}", customerNIC);
                
                var customer = _dbContext.Get<Customer>(c => c.CustomerNIC == customerNIC && c.DeletedAt == null)
                                         .Include(c => c.Transactions)
                                         .FirstOrDefault();
                
                if (customer == null)
                {
                    Log.Warning("Customer not found with NIC: {CustomerNIC}", customerNIC);
                }
                else
                {
                    Log.Debug("Found customer: {CustomerName} with NIC: {CustomerNIC}", customer.CustomerName, customerNIC);
                }
                
                return customer;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching customer with NIC: {CustomerNIC}", customerNIC);
                throw;
            }
        }

        public int? GetCustomerCount()
        {
            try
            {
                var count = _dbContext.Get<Customer>(c => c.DeletedAt == null).Count();
                Log.Information("Total active customers: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting customer count");
                throw;
            }
        }
    }
}
