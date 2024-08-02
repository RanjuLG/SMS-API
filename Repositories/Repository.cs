using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SMS.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SMS.Repositories
{
    public class Repository<TContext> : ReadOnlyRepository<TContext>, IRepository where TContext : DbContext
    {
        public Repository(TContext context) : base(context)
        {
        }

        public IDbContextTransaction CreateTransaction()
        {
            return _context.Database.BeginTransaction();
        }

        public void RollbackTransaction()
        {
            _context.Database.RollbackTransaction();
        }

        public void CommitTransaction()
        {
            _context.Database.CommitTransaction();
        }

        public void Create<TEntity>(TEntity entity, string createdBy = null) where TEntity : class
        {
            _context.Set<TEntity>().Add(entity);
        }

        public void CreateList<TEntity>(List<TEntity> entities, string createdBy = null) where TEntity : class
        {
            _context.Set<TEntity>().AddRange(entities);
        }

        public void Delete<TEntity>(object id) where TEntity : class
        {
            var entity = _context.Set<TEntity>().Find(id);
            if (entity != null)
            {
                _context.Set<TEntity>().Remove(entity);
            }
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public IQueryable<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> predicate = null) where TEntity : class
        {
            return predicate == null ? _context.Set<TEntity>() : _context.Set<TEntity>().Where(predicate);
        }

        public IList<TEntity> GetDataSqlQuery<TEntity>(string query) where TEntity : class
        {
            return _context.Set<TEntity>().FromSqlRaw(query).ToList();
        }

        public void Reload<TEntity>(object id) where TEntity : class
        {
            var entity = _context.Set<TEntity>().Find(id);
            if (entity != null)
            {
                _context.Entry(entity).Reload();
            }
        }

        public void Save()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                ThrowEnhancedValidationException(e);
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbEntityValidationException e)
            {
                ThrowEnhancedValidationException(e);
            }
        }

        public void SqlQuery<TEntity>(string sqlQuery) where TEntity : class
        {
            _context.Set<TEntity>().FromSqlRaw(sqlQuery).ToList();
        }

        public void SqlQuery(string sqlQuery)
        {
            _context.Database.ExecuteSqlRaw(sqlQuery);
        }

        public void Update<TEntity>(TEntity entity, object modified = null) where TEntity : class
        {
            _context.Set<TEntity>().Attach(entity);

            if (modified == null)
            {
                _context.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                var entry = _context.Entry(entity);
                foreach (PropertyInfo property in modified.GetType().GetProperties())
                {
                    entry.Property(property.Name).IsModified = true;
                }
            }
        }

        protected virtual void ThrowEnhancedValidationException(DbEntityValidationException e)
        {
            var errorMessages = e.EntityValidationErrors
                .SelectMany(x => x.ValidationErrors)
                .Select(x => x.ErrorMessage);

            var fullErrorMessage = string.Join("; ", errorMessages);
            var exceptionMessage = string.Concat(e.Message, " The validation errors are: ", fullErrorMessage);
            throw new DbEntityValidationException(exceptionMessage, e.EntityValidationErrors);
        }
    }
}
