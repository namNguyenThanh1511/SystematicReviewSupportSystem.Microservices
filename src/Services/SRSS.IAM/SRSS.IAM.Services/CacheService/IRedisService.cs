namespace SRSS.IAM.Services.CacheService
{
    public interface IRedisService
    {
        Task SetAsync(string key, string value, TimeSpan? expiry = null);
        Task<string?> GetAsync(string key);
        Task RemoveAsync(string key);

        Task<bool> ExistsAsync(string key);

        Task<TimeSpan> GetTTLAysnc(string key);
    }
}
