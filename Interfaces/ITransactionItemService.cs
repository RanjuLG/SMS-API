using SMS.Models;

namespace SMS.Interfaces
{
    public interface ITransactionItemService
    {
        IList<TransactionItem> GetAllTransactionItems();
        TransactionItem GetTransactionItemById(int transactionItemId);

        IList<TransactionItem> GetTransactionItemsByTransactionId(int transactionId);
        void CreateTransactionItem(TransactionItem transactionItem);
        void UpdateTransactionItem(TransactionItem transactionItem);
        void DeleteTransactionItem(int transactionItemId);
        void DeleteTransactionItems(IEnumerable<int> transactionItemIds);
    }
}
