using System.Collections.Generic;

namespace TwitchLiveStreamsNotifier.Application.Config
{
    public class TwitchConfig
    {
        public string ClientId { get; set; }

        public List<string> Logins { get; set; }
    }
}
