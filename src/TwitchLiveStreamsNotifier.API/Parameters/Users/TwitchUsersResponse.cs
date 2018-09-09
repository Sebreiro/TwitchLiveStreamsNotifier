using System.Collections.Generic;

namespace TwitchLiveStreamsNotifier.API.Parameters.Users
{
    public class TwitchUsersResponse
    {
        public List<TwitchUserData> Data;
    }

    public class TwitchUserData
    {
        public string Id;
        public string Login;
        public string Display_name;
        public string Type;
        public string Broadcaster_type;
        public string Description;
        public string Email;
    }
}
