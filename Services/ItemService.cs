using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Castle.Core.Resource;
using SMS.Enums;
using SMS.Interfaces;
using SMS.Models;

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

        public Item GetItemById(int itemId)
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
