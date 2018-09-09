namespace TwitchLiveStreamsNotifier.LiveStreamsCache.Config
{
    public class StreamCacheConfig
    {
        public int TimeBetweenNewStreams { get; set; } = 9;
        public int ClearCacheTime { get; set; } = 60;
    }
}
