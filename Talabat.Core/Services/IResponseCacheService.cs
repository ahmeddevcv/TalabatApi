using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Services
{
    public interface IResponseCacheService
    {
        Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeToLive);
        Task<string?> GetCachedResponseAsync(string cacheKey);
        /// <summary>Deletes every cached response whose key matches the pattern (e.g. "/api/products*").</summary>
        Task RemoveCacheByPatternAsync(string pattern);
    }
}
