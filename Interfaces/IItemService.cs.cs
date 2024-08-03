using SMS.Models;
using System.Collections.Generic;

namespace SMS.Interfaces
{
    public interface IItemService
    {
        IList<Item> GetAllItems();
        Item GetItemById(int itemId);
        void CreateItem(Item item);
        void UpdateItem(Item item);
        void DeleteItem(int itemId);
        void DeleteItems(IEnumerable<int> itemIds);
        IEnumerable<Item> GetItemsByCustomerId(int customerId);

    }
}
