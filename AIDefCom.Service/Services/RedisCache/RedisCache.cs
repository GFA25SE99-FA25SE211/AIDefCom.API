using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.RedisCache
{
    public class RedisCache : IRedisCache
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly ILogger<RedisCache> _logger;
        private readonly int _defaultTtl;

        public RedisCache(
            IConnectionMultiplexer redis,
            IConfiguration config,
            ILogger<RedisCache> logger)
        {
            _redis = redis;
            _logger = logger;
            
            var dbIndex = config.GetValue<int>("Redis:Database", 0);
            _db = _redis.GetDatabase(dbIndex);
            
            _defaultTtl = config.GetValue<int>("Redis:TtlSeconds", 3600);
        }

        public async Task<string?> GetAsync(string key)
        {
            try
            {
                var value = await _db.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                {
                    _logger.LogDebug("?? Redis key not found: {Key}", key);
                    return null;
                }

                _logger.LogInformation("? Retrieved from Redis: {Key}", key);
                return value.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error getting Redis key: {Key}", key);
                throw;
            }
        }

        public async Task SetAsync(string key, string value, int? ttlSeconds = null)
        {
            try
            {
                var expiry = TimeSpan.FromSeconds(ttlSeconds ?? _defaultTtl);
                await _db.StringSetAsync(key, value, expiry);
                _logger.LogInformation("?? Saved to Redis: {Key} (TTL: {Ttl}s)", key, expiry.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error setting Redis key: {Key}", key);
                throw;
            }
        }

        public async Task DeleteAsync(string key)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
                _logger.LogInformation("??? Deleted from Redis: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error deleting Redis key: {Key}", key);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error checking Redis key existence: {Key}", key);
                throw;
            }
        }
    }
}
