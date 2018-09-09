using TwitchLiveStreamsNotifier.LiveStreamsCache.Parameters;

namespace TwitchLiveStreamsNotifier.LiveStreamsCache.Services
{
    public interface ILiveStreamsCacheManager
    {
        bool IsNewStream(StreamData data);
    }
}