using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CogMediHospitalManagementSystem.Repositories
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        T? Get(Expression<Func<T, bool>> predicate);
        T? GetById(int id);
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void Delete(T entity);
        bool SaveChanges();
    }
}
