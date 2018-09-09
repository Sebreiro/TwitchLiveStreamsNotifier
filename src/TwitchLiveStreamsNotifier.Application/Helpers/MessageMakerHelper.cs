using System;
using System.Collections.Generic;
using System.Linq;
using TwitchLiveStreamsNotifier.Application.Streams.Parameters;

namespace TwitchLiveStreamsNotifier.Application.Helpers
{
    public static class MessageMakerHelper
    {
        public static List<string> CreateStreamMessages(List<LiveStreamData> liveStreamsData)
        {
            if (liveStreamsData == null)
                throw new InvalidOperationException("FollowedStreamsResponse dto is null");

            return liveStreamsData.Count == 0 
                ? null 
                : liveStreamsData.Select(CreateStreamMessage).ToList();
        }

        public static string CreateStreamMessage(LiveStreamData liveStreamData)
        {
            return $"{liveStreamData.UserName}; {liveStreamData.GameName}; {liveStreamData.StartedAt}; {liveStreamData.Type}; {liveStreamData.StreamName}; {liveStreamData.ChannelUrl}";
        }
    }
}
