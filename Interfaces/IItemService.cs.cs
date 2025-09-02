using SMS.Models;
using SMS.Models.DTO;
using System.Collections.Generic;

namespace SMS.Interfaces
{
    public interface IItemService
    {
        IList<Item> GetAllItems(IDateTimeRange dateTimeRange);
        IQueryable<Item> GetItemsQueryable(ItemSearchRequest request);
        Item? GetItemById(int itemId);
        void CreateItem(Item item);
        void UpdateItem(Item item);
        void DeleteItem(int itemId);
        void DeleteItems(IEnumerable<int> itemIds);
        IEnumerable<Item> GetItemsByCustomerId(int customerId);

        int? GetInventoryCount();

    }
}
