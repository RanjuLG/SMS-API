using Microsoft.EntityFrameworkCore.Storage;

namespace SMS.Repositories
{
    public class TransactionWrapper : IDisposable
    {
        private readonly IDbContextTransaction _transaction;
        private readonly bool _isOwner;

        public TransactionWrapper(IDbContextTransaction transaction, bool isOwner)
        {
            _transaction = transaction;
            _isOwner = isOwner;
        }

        public void Commit()
        {
            if (_isOwner)
                _transaction.Commit();
        }

        public void Dispose()
        {
            if (_isOwner)
                _transaction.Dispose();
        }

        public void Rollback()
        {
            if (_isOwner) _transaction.Rollback();
        }
    }
}
