using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Castle.Core.Resource;
using SMS.Enums;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO;
using Serilog;

namespace SMS.Services
{
    public class ItemService : IItemService
    {
        private readonly IRepository _dbContext;
        private readonly IReadOnlyRepository _readOnlyRepository;

        public ItemService(IRepository dbContext, IReadOnlyRepository readOnlyRepository)
        {
            _dbContext = dbContext;
            _readOnlyRepository = readOnlyRepository;
        }

        public IList<Item> GetAllItems(IDateTimeRange dateTimeRange)
        {
            try
            {
                var startTime = dateTimeRange.From;
                var endTime = dateTimeRange.To;
                
                Log.Information("Fetching items from {StartTime} to {EndTime}", startTime, endTime);
                
                var items = _dbContext.Get<Item>(i => i.DeletedAt == null && i.CreatedAt <= endTime && i.CreatedAt >= startTime)
                                 .Include(i => i.Customer) // Include Customer entity
                                 .ToList();
                
                Log.Information("Retrieved {Count} items", items.Count);
                return items;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching items in date range {StartTime} to {EndTime}", 
                    dateTimeRange.From, dateTimeRange.To);
                throw;
            }
        }

        public IQueryable<Item> GetItemsQueryable(ItemSearchRequest request)
        {
            try
            {
                Log.Information("Searching items with query: Search={Search}, CustomerNIC={CustomerNIC}, MinValue={MinValue}, MaxValue={MaxValue}", 
                    request.Search, request.CustomerNIC, request.MinValue, request.MaxValue);
                
                // Base query for non-deleted items
                var query = _dbContext.Get<Item>(i => i.DeletedAt == null)
                                      .Include(i => i.Customer);

                // Apply date range filter only if dates are provided
                if (request.From.HasValue && request.To.HasValue)
                {
                    query = query.Where(i => i.CreatedAt >= request.From.Value && i.CreatedAt <= request.To.Value);
                }
                else if (request.From.HasValue)
                {
                    query = query.Where(i => i.CreatedAt >= request.From.Value);
                }
                else if (request.To.HasValue)
                {
                    query = query.Where(i => i.CreatedAt <= request.To.Value);
                }

                // Apply enhanced search filter
                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    var searchTerm = request.Search.ToLower();
                    query = query.Where(i => 
                        (i.ItemDescription != null && i.ItemDescription.ToLower().Contains(searchTerm)) ||
                        (i.ItemRemarks != null && i.ItemRemarks.ToLower().Contains(searchTerm)) ||
                        (i.Customer != null && i.Customer.CustomerNIC != null && i.Customer.CustomerNIC.ToLower().Contains(searchTerm)) ||
                        (i.Customer != null && i.Customer.CustomerName != null && i.Customer.CustomerName.ToLower().Contains(searchTerm)) ||
                        (i.Customer != null && i.Customer.CustomerContactNo != null && i.Customer.CustomerContactNo.Contains(searchTerm)) ||
                        (i.Customer != null && i.Customer.CustomerAddress != null && i.Customer.CustomerAddress.ToLower().Contains(searchTerm)) ||
                        (i.ItemCaratage.HasValue && i.ItemCaratage.Value.ToString().Contains(searchTerm)) ||
                        (i.ItemWeight.HasValue && i.ItemWeight.Value.ToString().Contains(searchTerm)) ||
                        (i.ItemGoldWeight.HasValue && i.ItemGoldWeight.Value.ToString().Contains(searchTerm)) ||
                        (i.ItemValue.HasValue && i.ItemValue.Value.ToString().Contains(searchTerm)) ||
                        i.ItemId.ToString().Contains(searchTerm)
                    );
                }

                // Apply customer NIC filter
                if (!string.IsNullOrWhiteSpace(request.CustomerNIC))
                {
                    query = query.Where(i => i.Customer != null && i.Customer.CustomerNIC == request.CustomerNIC);
                }

                // Apply value range filters
                if (request.MinValue.HasValue)
                {
                    query = query.Where(i => i.ItemValue >= request.MinValue.Value);
                }

                if (request.MaxValue.HasValue)
                {
                    query = query.Where(i => i.ItemValue <= request.MaxValue.Value);
                }

                // Apply sorting with more options
                if (!string.IsNullOrWhiteSpace(request.SortBy))
                {
                    switch (request.SortBy.ToLower())
                    {
                        case "itemdescription":
                            query = request.SortOrder?.ToLower() == "desc" 
                                ? query.OrderByDescending(i => i.ItemDescription)
                                : query.OrderBy(i => i.ItemDescription);
                            break;
                        case "itemvalue":
                            query = request.SortOrder?.ToLower() == "desc"
                                ? query.OrderByDescending(i => i.ItemValue)
                                : query.OrderBy(i => i.ItemValue);
                            break;
                        case "createdat":
                            query = request.SortOrder?.ToLower() == "desc"
                                ? query.OrderByDescending(i => i.CreatedAt)
                                : query.OrderBy(i => i.CreatedAt);
                            break;
                        case "customernic":
                            query = request.SortOrder?.ToLower() == "desc"
                                ? query.OrderByDescending(i => i.Customer.CustomerNIC)
                                : query.OrderBy(i => i.Customer.CustomerNIC);
                            break;
                        case "customername":
                            query = request.SortOrder?.ToLower() == "desc"
                                ? query.OrderByDescending(i => i.Customer.CustomerName)
                                : query.OrderBy(i => i.Customer.CustomerName);
                            break;
                        case "itemweight":
                            query = request.SortOrder?.ToLower() == "desc"
                                ? query.OrderByDescending(i => i.ItemWeight)
                                : query.OrderBy(i => i.ItemWeight);
                            break;
                        case "itemgoldweight":
                            query = request.SortOrder?.ToLower() == "desc"
                                ? query.OrderByDescending(i => i.ItemGoldWeight)
                                : query.OrderBy(i => i.ItemGoldWeight);
                            break;
                        case "itemcaratage":
                            query = request.SortOrder?.ToLower() == "desc"
                                ? query.OrderByDescending(i => i.ItemCaratage)
                                : query.OrderBy(i => i.ItemCaratage);
                            break;
                        default:
                            query = query.OrderByDescending(i => i.CreatedAt);
                            break;
                    }
                }
                else
                {
                    query = query.OrderByDescending(i => i.CreatedAt);
                }

                return query;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error building item query");
                throw;
            }
        }

        public Item? GetItemById(int itemId)
        {
            try
            {
                Log.Debug("Fetching item with ID: {ItemId}", itemId);
                
                var item = _dbContext.Get<Item>(i => i.ItemId == itemId && i.DeletedAt == null)
                                 .Include(i => i.Customer)
                                 .FirstOrDefault();
                
                if (item == null)
                {
                    Log.Warning("Item not found with ID: {ItemId}", itemId);
                }
                
                return item;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching item with ID: {ItemId}", itemId);
                throw;
            }
        }

        public void CreateItem(Item item)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    Log.Information("Creating new item: {ItemDescription}, Value: {ItemValue}, Customer ID: {CustomerId}", 
                        item.ItemDescription, item.ItemValue, item.CustomerId);
                    
                    _dbContext.Create<Item>(item);
                    _dbContext.Save();
                    _dbContext.CommitTransaction();
                    
                    Log.Information("Item created successfully with ID: {ItemId}", item.ItemId);
                }
                catch (Exception ex)
                {
                    _dbContext.RollbackTransaction();
                    Log.Error(ex, "Error creating item: {ItemDescription}", item.ItemDescription);
                    throw;
                }
            }
        }

        public void UpdateItem(Item item)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    Log.Information("Updating item ID: {ItemId}, Description: {ItemDescription}", 
                        item.ItemId, item.ItemDescription);
                    
                    item.UpdatedAt = DateTime.Now;
                    _dbContext.Update<Item>(item);
                    _dbContext.Save();
                    _dbContext.CommitTransaction();
                    
                    Log.Information("Item {ItemId} updated successfully", item.ItemId);
                }
                catch (Exception ex)
                {
                    _dbContext.RollbackTransaction();
                    Log.Error(ex, "Error updating item ID: {ItemId}", item.ItemId);
                    throw;
                }
            }
        }

        public void DeleteItem(int itemId)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    Log.Information("Soft deleting item ID: {ItemId}", itemId);
                    
                    var item = _dbContext.GetById<Item>(itemId);
                    if (item != null)
                    {
                        item.DeletedAt = DateTime.Now;
                        _dbContext.Update<Item>(item);
                        _dbContext.Save();
                        Log.Information("Item {ItemDescription} (ID: {ItemId}) soft deleted successfully", 
                            item.ItemDescription, itemId);
                    }
                    else
                    {
                        Log.Warning("Attempted to delete non-existent item ID: {ItemId}", itemId);
                    }
                    
                    _dbContext.CommitTransaction();
                }
                catch (Exception ex)
                {
                    _dbContext.RollbackTransaction();
                    Log.Error(ex, "Error deleting item ID: {ItemId}", itemId);
                    throw;
                }
            }
        }

        public void DeleteItems(IEnumerable<int> itemIds)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    var ids = itemIds.ToList();
                    Log.Information("Soft deleting {Count} items", ids.Count);
                    
                    var items = _dbContext.Get<Item>(i => itemIds.Contains(i.ItemId) && i.DeletedAt == null).ToList();
                    foreach (var item in items)
                    {
                        item.DeletedAt = DateTime.Now;
                        _dbContext.Update<Item>(item);
                        Log.Debug("Marking item {ItemDescription} (ID: {ItemId}) as deleted", 
                            item.ItemDescription, item.ItemId);
                    }
                    _dbContext.Save();
                    _dbContext.CommitTransaction();
                    
                    Log.Information("Successfully soft deleted {DeletedCount} of {RequestedCount} items", 
                        items.Count, ids.Count);
                }
                catch (Exception ex)
                {
                    _dbContext.RollbackTransaction();
                    Log.Error(ex, "Error deleting multiple items");
                    throw;
                }
            }
        }

        public IEnumerable<Item> GetItemsByCustomerId(int customerId)
        {
            try
            {
                Log.Debug("Fetching items for customer ID: {CustomerId}", customerId);
                
                var items = _dbContext.Get<Item>(i => i.CustomerId == customerId && i.DeletedAt == null).ToList();
                
                Log.Information("Retrieved {Count} items for customer ID: {CustomerId}", items.Count, customerId);
                return items;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching items for customer ID: {CustomerId}", customerId);
                throw;
            }
        }


        public int? GetInventoryCount()
        {
            try
            {
                var count = _dbContext.Get<Item>(i => i.DeletedAt == null && (i.Status == 1 || i.Status == 3) ).Count();
                Log.Information("Total inventory count: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting inventory count");
                throw;
            }
        }
    }
}
