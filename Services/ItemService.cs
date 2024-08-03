using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

        public IList<Item> GetAllItems()
        {
            return _dbContext.Get<Item>(i => i.DeletedAt == null)
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
            _dbContext.Create<Item>(item);
            _dbContext.Save();
        }

        public void UpdateItem(Item item)
        {
            item.UpdatedAt = DateTime.Now;
            _dbContext.Update<Item>(item);
            _dbContext.Save();
        }

        public void DeleteItem(int itemId)
        {
            var item = _dbContext.GetById<Item>(itemId);
            if (item != null)
            {
                item.DeletedAt = DateTime.Now;
                _dbContext.Update<Item>(item);
                _dbContext.Save();
            }
        }

        public void DeleteItems(IEnumerable<int> itemIds)
        {
            var items = _dbContext.Get<Item>(i => itemIds.Contains(i.ItemId) && i.DeletedAt == null).ToList();
            foreach (var item in items)
            {
                item.DeletedAt = DateTime.Now;
                _dbContext.Update<Item>(item);
            }
            _dbContext.Save();
        }

        public IEnumerable<Item> GetItemsByCustomerId(int customerId)
        {
            return _dbContext.Get<Item>(i => i.CustomerId == customerId && i.DeletedAt == null).ToList();
        }

    }
}
