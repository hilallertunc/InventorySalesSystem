using System;
using System.Threading.Tasks;

namespace InventorySales.Application.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpireTime = null, TimeSpan? slidingExpireTime = null);
        Task RemoveAsync(string key);
    }
}