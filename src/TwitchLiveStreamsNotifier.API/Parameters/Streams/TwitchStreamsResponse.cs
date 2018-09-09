using System.Collections.Generic;

namespace TwitchLiveStreamsNotifier.API.Parameters.Streams
{
    public class TwitchStreamsResponse
    {
        public List<TwitchStreamData> Data;
    }

    public class TwitchStreamData
    {
        public string Id;
        public string User_id;
        public string Game_id;
        //Stream type: "live" or "" (in case of premiere video).
        public string Type;
        public string Title;
        public string Viewer_count;
        public string Started_at;
    }


    
}
