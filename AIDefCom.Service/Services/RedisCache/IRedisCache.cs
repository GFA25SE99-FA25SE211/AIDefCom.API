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
    }
}
