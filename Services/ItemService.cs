using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Castle.Core.Resource;
using SMS.Enums;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.DTO;

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
             var startTime = dateTimeRange.From;
            var endTime = dateTimeRange.To;
            return _dbContext.Get<Item>(i => i.DeletedAt == null && i.CreatedAt <= endTime && i.CreatedAt >= startTime)
                             .Include(i => i.Customer) // Include Customer entity
                             .ToList();
        }

        public IQueryable<Item> GetItemsQueryable(ItemSearchRequest request)
        {
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

        public Item? GetItemById(int itemId)
        {
            return _dbContext.Get<Item>(i => i.ItemId == itemId && i.DeletedAt == null)
                             .Include(i => i.Customer)
                             .FirstOrDefault();
        }

        public void CreateItem(Item item)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    _dbContext.Create<Item>(item);
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

        public void UpdateItem(Item item)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    item.UpdatedAt = DateTime.Now;
                    _dbContext.Update<Item>(item);
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

        public void DeleteItem(int itemId)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    var item = _dbContext.GetById<Item>(itemId);
                    if (item != null)
                    {
                        item.DeletedAt = DateTime.Now;
                        _dbContext.Update<Item>(item);
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

        public void DeleteItems(IEnumerable<int> itemIds)
        {
            using (var dbTransaction = _dbContext.CreateTransaction())
            {
                try
                {
                    var items = _dbContext.Get<Item>(i => itemIds.Contains(i.ItemId) && i.DeletedAt == null).ToList();
                    foreach (var item in items)
                    {
                        item.DeletedAt = DateTime.Now;
                        _dbContext.Update<Item>(item);
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

        public IEnumerable<Item> GetItemsByCustomerId(int customerId)
        {
            return _dbContext.Get<Item>(i => i.CustomerId == customerId && i.DeletedAt == null).ToList();
        }


        public int? GetInventoryCount()
        {

            return _dbContext.Get<Item>(i => i.DeletedAt == null && (i.Status == 1 || i.Status == 3) ).Count();

        }
    }
}
