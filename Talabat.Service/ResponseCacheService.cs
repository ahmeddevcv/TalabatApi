using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Talabat.Core.Services;

namespace Talabat.Service
{
    public class ResponseCacheService : IResponseCacheService
    {
        //inject redis here of type IconnectioMultiplexer
        private readonly IDatabase _database;
        public ResponseCacheService( IConnectionMultiplexer redis ) 
        {
            _database = redis.GetDatabase();
        }


        public Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeToLive)
        {
            if (response == null) return Task.CompletedTask;
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var serializedResponse = JsonSerializer.Serialize(response,options);
            return _database.StringSetAsync(cacheKey, serializedResponse, timeToLive);

        }

        public async Task<string?> GetCachedResponseAsync(string cacheKey)
        {

            var cachedResponse =await _database.StringGetAsync(cacheKey);
            if(cachedResponse.IsNullOrEmpty) return null;
            return cachedResponse;

        }
    }
}
