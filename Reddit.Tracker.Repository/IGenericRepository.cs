using Reddit.Tracker.Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reddit.Tracker.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<Dictionary<string, T>> GetAll();
        Task Add(string key, T entity);
        Task Delete(string key);
        Task Update(string key, T entity);
    }
}
