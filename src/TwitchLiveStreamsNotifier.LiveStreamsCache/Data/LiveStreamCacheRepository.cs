using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TwitchLiveStreamsNotifier.LiveStreamsCache.Parameters;

namespace TwitchLiveStreamsNotifier.LiveStreamsCache.Data
{
    public class LiveStreamCacheRepository:ILiveStreamRepository
    {
        private ILogger _logger;


        private Dictionary<string, StreamRepositoryData> _storage;
        public LiveStreamCacheRepository(ILogger<LiveStreamCacheRepository> logger)
        {
            _logger = logger;

            _storage = new Dictionary<string, StreamRepositoryData>();
        }

        public void AddOrUpdate(string streamerName, StreamData data)
        {
            if (string.IsNullOrWhiteSpace(streamerName))
                throw new ArgumentException($"{nameof(streamerName)} is null");

            var cacheData = new StreamRepositoryData()
            {
                StreamName = data.StreamName,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            _storage[streamerName] = cacheData;
        }

        public void UpdateTime(string streamerName)
        {
            var data = Get(streamerName);
            data.Updated = DateTime.UtcNow;
        }

        public StreamRepositoryData Get(string streamerName)
        {
            if (string.IsNullOrWhiteSpace(streamerName))
                throw new ArgumentException($"{nameof(streamerName)} is null");
            
            return _storage.TryGetValue(streamerName, out var data) ? data : null;
        }
    }
}
