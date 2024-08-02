using SMS.Models;

namespace SMS.Interfaces
{
    public interface ITransactionItemService
    {
        IList<TransactionItem> GetAllTransactionItems();
        TransactionItem GetTransactionItemById(int transactionItemId);
        void CreateTransactionItem(TransactionItem transactionItem);
        void UpdateTransactionItem(TransactionItem transactionItem);
        void DeleteTransactionItem(int transactionItemId);
        void DeleteTransactionItems(IEnumerable<int> transactionItemIds);
    }
}
