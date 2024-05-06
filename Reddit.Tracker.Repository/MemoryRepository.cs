using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reddit.Tracker.Repository
{
    /// <summary>
    /// Generic In-memory repository to hold models for the application
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MemoryRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ConcurrentDictionary<string, T> _dbContext = new ConcurrentDictionary<string, T>();
        public async Task Add(string key, T entity)
        {
            await Task.Run(() => _dbContext.TryAdd(key, entity));
        }

        public async Task Delete(string key)
        {
            await Task.Run(() => _dbContext.TryRemove(key, out T val));
        }

        public Task<Dictionary<string, T>> GetAll()
        {
            return Task.Run(() => _dbContext.ToDictionary<string, T>());
        }

        public async Task Update(string key, T entity)
        {
            await Task.Run(() => Delete(key));
            await Task.Run(() => Add(key, entity));
        }
    }
}
