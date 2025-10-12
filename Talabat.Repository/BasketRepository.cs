using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Repositories;
using Talabat.Repository.Data;

namespace Talabat.Repository
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDatabase _database;
        public BasketRepository(IConnectionMultiplexer redis )//2injection redis
        {
            _database = redis.GetDatabase();
        }

        public Task<bool> DeleteBasketAsync(string basketId)
        {
            return _database.KeyDeleteAsync( basketId );
        }

        public async Task<CustmerBasket?> GetBasketAsync(string basketId)
        {
            var basket =await _database.StringGetAsync( basketId );
            return basket.IsNull ? null : JsonSerializer.Deserialize<CustmerBasket>(basket);
            //if (basket.IsNull) return null;
            //return JsonSerializer.Deserialize<CustmerBasket>(basket);
        }

        public async Task<CustmerBasket?> UpdateBasketAsync(CustmerBasket basket)
        {
            var creatOrUpdate = await _database.StringSetAsync(basket.Id,JsonSerializer.Serialize<CustmerBasket>(basket));
            if(!creatOrUpdate)return null;
            return await GetBasketAsync(basket.Id);
        }
    }
}
