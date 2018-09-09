using System.Collections.Generic;

namespace TwitchLiveStreamsNotifier.API.Parameters.Games
{
    public class TwitchGamesResponse
    {
        public List<TwitchUserData> Data;
    }

    public class TwitchUserData
    {
        public string Id;
        public string Name;
    }
}
