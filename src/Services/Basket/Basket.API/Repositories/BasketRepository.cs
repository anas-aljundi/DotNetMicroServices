using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDistributedCache _redisCache;
        private readonly ILogger<BasketRepository> _logger;
        public BasketRepository(IDistributedCache redisCache, ILogger<BasketRepository> logger)
        {
            _redisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task DeleteBasket(string userName)
        {
            var basket = await GetBasket(userName);
            _logger.LogInformation($"user: {userName} deletes cart {basket}");
            await _redisCache.RemoveAsync(userName);
        }
        public async Task<ShoppingCart> GetBasket(string userName)
        {
            var basket = await _redisCache.GetStringAsync(userName);
            if (string.IsNullOrEmpty(basket))
                return null;
            ShoppingCart cart = JsonConvert.DeserializeObject<ShoppingCart>(basket);
            _logger.LogInformation($"user: {userName} retrieves cart: {cart}");
            return cart;
        }
        public async Task<ShoppingCart> UpdateBasket(ShoppingCart basket)
        {
            var oldBasket = await _redisCache.GetStringAsync(basket.UserName);
            if (string.IsNullOrEmpty(oldBasket))
                _logger.LogInformation($"user: {basket.UserName} saves cart: {basket}");
            else
            {
                _logger.LogInformation($"user: {basket.UserName} updates cart: {oldBasket} with the new cart: {basket}");
            }
            await _redisCache.SetStringAsync(basket.UserName, JsonConvert.SerializeObject(basket));
            return await GetBasket(basket.UserName);
        }
    }
}
