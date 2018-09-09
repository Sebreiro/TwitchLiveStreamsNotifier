using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchLiveStreamsNotifier.LiveStreamsCache.Config;
using TwitchLiveStreamsNotifier.LiveStreamsCache.Data;
using TwitchLiveStreamsNotifier.LiveStreamsCache.Parameters;

namespace TwitchLiveStreamsNotifier.LiveStreamsCache.Services
{
    public class LiveStreamsCacheManager: ILiveStreamsCacheManager
    {
        private readonly ILogger _logger;
        private readonly ILiveStreamRepository _repository;
        private readonly IOptionsMonitor<StreamCacheConfig> _configMonitor;

        public LiveStreamsCacheManager(ILogger<LiveStreamsCacheManager> logger,
            ILiveStreamRepository repository,
            IOptionsMonitor<StreamCacheConfig> configMonitor)
        {
            _logger = logger;
            _repository = repository;
            _configMonitor = configMonitor;
        }

        public bool IsNewStream(StreamData data)
        {
            if (data == null)
                throw new ArgumentException($"{nameof(data)} is null");

            var config = GetConfig();

            var result = _repository.Get(data.StreamerName);

            if (result == null)
            {
                _logger.LogInformation($"New Streamer {data.StreamerName} Added to the cache");

                _repository.AddOrUpdate(data.StreamerName, data);
                return true;
            }

            if (result.StreamName != data.StreamName || IsOldStream(result.Updated, config.TimeBetweenNewStreams))
            {
                _logger.LogInformation($"Streamer {data.StreamerName} started a new stream: {data.StreamName}");

                _repository.AddOrUpdate(data.StreamerName, data);
                return true;
            }

            _repository.UpdateTime(data.StreamerName);

            _logger.LogInformation($"Already streaming: {data.StreamerName}; StreamName: {data.StreamName}");

            return false;
        }

        private bool IsOldStream(DateTimeOffset lastUpdated, int timeBetweenStreams)
        {
            return (DateTime.UtcNow - lastUpdated).TotalMinutes > timeBetweenStreams;
        }

        private StreamCacheConfig GetConfig()
        {
            var config = _configMonitor.CurrentValue;
            return config;
        }
    }
}
