using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    _logger.LogDebug("🔍 Redis key not found: {Key}", key);
                    return null;
                }

                _logger.LogInformation("✅ Retrieved from Redis: {Key}", key);
                return value.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting Redis key: {Key}", key);
                throw;
            }
        }

        public async Task SetAsync(string key, string value, int? ttlSeconds = null)
        {
            try
            {
                var expiry = TimeSpan.FromSeconds(ttlSeconds ?? _defaultTtl);
                await _db.StringSetAsync(key, value, expiry);
                _logger.LogInformation("💾 Saved to Redis: {Key} (TTL: {Ttl}s)", key, expiry.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error setting Redis key: {Key}", key);
                throw;
            }
        }

        public async Task DeleteAsync(string key)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
                _logger.LogInformation("🗑️ Deleted from Redis: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error deleting Redis key: {Key}", key);
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
                _logger.LogError(ex, "❌ Error checking Redis key existence: {Key}", key);
                throw;
            }
        }

        public async Task<Dictionary<string, string>?> GetHashAllAsync(string key)
        {
            try
            {
                var hashEntries = await _db.HashGetAllAsync(key);
                
                if (hashEntries == null || hashEntries.Length == 0)
                {
                    _logger.LogDebug("🔍 Redis hash not found: {Key}", key);
                    return null;
                }

                var result = hashEntries.ToDictionary(
                    entry => entry.Name.ToString(),
                    entry => entry.Value.ToString()
                );

                _logger.LogInformation("✅ Retrieved hash from Redis: {Key} ({Count} fields)", key, result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting Redis hash: {Key}", key);
                throw;
            }
        }

        public async Task<string?> GetHashFieldAsync(string key, string field)
        {
            try
            {
                var value = await _db.HashGetAsync(key, field);
                
                if (value.IsNullOrEmpty)
                {
                    _logger.LogDebug("🔍 Redis hash field not found: {Key}.{Field}", key, field);
                    return null;
                }

                _logger.LogInformation("✅ Retrieved hash field from Redis: {Key}.{Field}", key, field);
                return value.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting Redis hash field: {Key}.{Field}", key, field);
                throw;
            }
        }

        public async Task SetHashAsync(string key, Dictionary<string, string> hashEntries, int? ttlSeconds = null)
        {
            try
            {
                var entries = hashEntries.Select(kvp => new HashEntry(kvp.Key, kvp.Value)).ToArray();
                await _db.HashSetAsync(key, entries);
                
                if (ttlSeconds.HasValue && ttlSeconds.Value > 0)
                {
                    var expiry = TimeSpan.FromSeconds(ttlSeconds.Value);
                    await _db.KeyExpireAsync(key, expiry);
                    _logger.LogInformation("💾 Saved hash to Redis: {Key} ({Count} fields, TTL: {Ttl}s)", 
                        key, hashEntries.Count, expiry.TotalSeconds);
                }
                else
                {
                    _logger.LogInformation("💾 Saved hash to Redis: {Key} ({Count} fields, no TTL)", 
                        key, hashEntries.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error setting Redis hash: {Key}", key);
                throw;
            }
        }

        public async Task<List<string>> GetKeysByPatternAsync(string pattern)
        {
            try
            {
                var endpoints = _redis.GetEndPoints();
                var server = _redis.GetServer(endpoints.First());
                
                var keys = server.Keys(pattern: pattern)
                    .Select(key => key.ToString())
                    .ToList();

                _logger.LogInformation("🔍 Found {Count} keys matching pattern: {Pattern}", keys.Count, pattern);
                return keys;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting keys by pattern: {Pattern}", pattern);
                throw;
            }
        }
    }
}
