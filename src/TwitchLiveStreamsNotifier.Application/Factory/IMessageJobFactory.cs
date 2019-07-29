using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TwitchLiveStreamsNotifier.Application.Factory
{
    public interface IMessageJobFactory
    {
        Func<Task<List<string>>> GetStreamsMessageJob();
    }
}