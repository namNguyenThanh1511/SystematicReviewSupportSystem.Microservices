using StackExchange.Redis;

namespace SRSS.IAM.Services.CacheService
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _db;
        public RedisService(IConnectionMultiplexer connectionMultiplexer)
        {
            _db = connectionMultiplexer.GetDatabase();
        }
        public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
            => await _db.StringSetAsync(key, value, expiry);

        public async Task<string?> GetAsync(string key)
            => await _db.StringGetAsync(key);

        public async Task RemoveAsync(string key)
            => await _db.KeyDeleteAsync(key);
        public async Task<bool> ExistsAsync(string key)
            => await _db.KeyExistsAsync(key);
        public async Task<TimeSpan> GetTTLAysnc(string key)
        {

            var ttl = await _db.KeyTimeToLiveAsync(key);
            if (ttl.HasValue && ttl.Value.TotalSeconds <= 0)
            {
                await _db.KeyDeleteAsync(key);
            }
            return ttl.Value;

        }

    }
}
