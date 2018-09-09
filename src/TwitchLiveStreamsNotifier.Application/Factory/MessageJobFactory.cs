using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLiveStreamsNotifier.Application.Helpers;
using TwitchLiveStreamsNotifier.Application.Streams;
using TwitchLiveStreamsNotifier.LiveStreamsCache.Parameters;
using TwitchLiveStreamsNotifier.LiveStreamsCache.Services;

namespace TwitchLiveStreamsNotifier.Application.Factory
{
    public class MessageJobFactory: IMessageJobFactory
    {
        private readonly ILiveStreamsCacheManager _streamsCache;
        private readonly ITwitchStreamsDataAggregator _dataAggregator;

        public MessageJobFactory(ITwitchStreamsDataAggregator dataAggregator, ILiveStreamsCacheManager streamsCache)
        {
            _dataAggregator = dataAggregator;
            _streamsCache = streamsCache;
        }

        public Func<Task<List<string>>> GetStreamsMesageJob()
        {
            Func<Task<List<string>>> job = async () =>
            {
                var liveStreamsData = await _dataAggregator.GetLiveStreamsData();
                if (liveStreamsData == null)
                    return null;

                var newLiveStreamsData = liveStreamsData
                    .FindAll(x => _streamsCache
                        .IsNewStream(new StreamData {StreamerName = x.UserName, StreamName = x.StreamName}));

                var messages = MessageMakerHelper.CreateStreamMessages(newLiveStreamsData);
                return messages;
            };

            return job;
        }
    }
}
