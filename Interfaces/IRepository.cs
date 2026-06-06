using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SMS.Interfaces
{
    public interface IRepository : IReadOnlyRepository
    {
        void Create<TEntity>(TEntity entity, string createdBy = null) where TEntity : class;

        void CreateList<TEntity>(List<TEntity> entities, string createdBy = null) where TEntity : class;

        void Update<TEntity>(TEntity entity, object modified = null) where TEntity : class;

        void Delete<TEntity>(object id) where TEntity : class;

        void Delete<TEntity>(TEntity entity) where TEntity : class;

        void Save();

        Task SaveAsync();

        void SqlQuery<TEntity>(string sqlQuery) where TEntity : class;

        void SqlQuery(string sqlQuery);

        void Reload<TEntity>(object id) where TEntity : class;

        IQueryable<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> predicate = null) where TEntity : class;

        IList<TEntity> GetDataSqlQuery<TEntity>(string query) where TEntity : class;

        IDbContextTransaction CreateTransaction();

        void RollbackTransaction();

        void CommitTransaction();
    }
}
