using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDefCom.Service.Services.RedisCache
{
    public interface IRedisCache
    {
        /// <summary>
        /// L?y giá tr? string t? Redis theo key
        /// </summary>
        Task<string?> GetAsync(string key);

        /// <summary>
        /// L?u giá tr? string vào Redis v?i TTL
        /// </summary>
        Task SetAsync(string key, string value, int? ttlSeconds = null);

        /// <summary>
        /// Xóa key kh?i Redis
        /// </summary>
        Task DeleteAsync(string key);

        /// <summary>
        /// Ki?m tra key có t?n t?i không
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// L?y t?t c? field và value t? một Redis hash
        /// </summary>
        Task<Dictionary<string, string>?> GetHashAllAsync(string key);

        /// <summary>
        /// L?y một field c? th? từ Redis hash
        /// </summary>
        Task<string?> GetHashFieldAsync(string key, string field);

        /// <summary>
        /// L?u hash vào Redis
        /// </summary>
        Task SetHashAsync(string key, Dictionary<string, string> hashEntries, int? ttlSeconds = null);

        /// <summary>
        /// L?y t?t c? keys theo pattern (ví d? transcript:*)
        /// </summary>
        Task<List<string>> GetKeysByPatternAsync(string pattern);
    }
}
