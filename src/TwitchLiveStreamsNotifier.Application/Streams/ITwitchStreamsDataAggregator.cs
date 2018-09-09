using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLiveStreamsNotifier.Application.Streams.Parameters;

namespace TwitchLiveStreamsNotifier.Application.Streams
{
    public interface ITwitchStreamsDataAggregator
    {
        Task<List<LiveStreamData>> GetLiveStreamsData();
    }
}