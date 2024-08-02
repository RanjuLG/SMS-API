using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SMS.Interfaces;

namespace SMS.Repositories
{
    public class ReadOnlyRepository<TContext> : IReadOnlyRepository where TContext : DbContext
    {

        protected readonly TContext _context;

        public ReadOnlyRepository(TContext context)
        {

            _context = context;


        }

        protected virtual IQueryable<TEntity> GetQueryable<TEntity>(
           Expression<Func<TEntity, bool>> filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
           string includeProperties = null,
           int? skip = null,
           int? take = null)
           where TEntity : class
        {
            includeProperties = includeProperties ?? string.Empty;

            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (filter != null)
            {

                query = query.Where(filter);

            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {

                query = query.Include(includeProperty);

            }

            if (orderBy != null)
            {

                query = orderBy(query);

            }

            if (skip.HasValue)
            {

                query = query.Skip(skip.Value);

            }

            if (take.HasValue)
            {

                query = query.Take(take.Value);

            }

            return query;

        }

        public IEnumerable<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null) where TEntity : class
        {

            return GetQueryable<TEntity>(filter, orderBy, includeProperties, skip, take).ToList();

        }

        public IEnumerable<TEntity> GetAll<TEntity>(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null) where TEntity : class
        {

            return GetQueryable<TEntity>(null, orderBy, includeProperties, skip, take).ToList();

        }

        public Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> GetAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public TEntity GetById<TEntity>(object id) where TEntity : class
        {

            return _context.Set<TEntity>().Find(id);

        }

        public Task<TEntity> GetByIdAsync<TEntity>(object id) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public bool GetConnetionStatus()
        {
            throw new NotImplementedException();
        }

        public int GetCount<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCountAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public bool GetExists<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetExistsAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public TEntity GetFirst<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> GetFirstAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public TEntity GetOne<TEntity>(Expression<Func<TEntity, bool>> filter = null, string includeProperties = null) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> GetOneAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, string includeProperties = null) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public ICollection<dynamic> Select<TEntity>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, dynamic>> select) where TEntity : class
        {
            throw new NotImplementedException();
        }
    }
}
