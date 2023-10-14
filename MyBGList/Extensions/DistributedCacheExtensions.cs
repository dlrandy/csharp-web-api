using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace MyBGList.Extensions
{
	public static class DistributedCacheExtensions
	{
		public static bool TryGetValue<T>(
			this IDistributedCache cache,
			string key,
			out T? value
			){
			value = default;
			var val = cache.Get(key);
			if (val == null)
			{
				return false;
			}
			value = JsonSerializer.Deserialize<T>(val);
			return true;

		}

		public static bool Set<T>(
			this IDistributedCache cache,
			string key,
			T value,
			TimeSpan absoluteExpirationRelativeToNow) {
			try
            {
                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
                cache.Set(key, bytes, new DistributedCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
                });

                return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		
		}
        public static async Task SetRecordAsync<T>(this IDistributedCache cache,
                                                  string recordId,
                                                  T data,
                                                  TimeSpan? absoluteExpireTime = null,
                                                  TimeSpan? slidingExpireTime = null)
        {
            var options = new DistributedCacheEntryOptions();

            options.AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromSeconds(60);
            options.SlidingExpiration = slidingExpireTime;

            var jsonData = JsonSerializer.Serialize(data);
            await cache.SetStringAsync(recordId, jsonData, options);
        }

        public static async Task<T?> GetRecordAsync<T>(this IDistributedCache cache,
                                                       string recordId)
        {
            var jsonData = await cache.GetStringAsync(recordId);

            if (jsonData is null)
            {
                return default(T);
            }

            return JsonSerializer.Deserialize<T>(jsonData);
        }
    }
}

