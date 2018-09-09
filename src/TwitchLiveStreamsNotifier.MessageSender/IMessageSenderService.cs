using System.Threading.Tasks;

namespace TwitchLiveStreamsNotifier.MessageSender
{
    public interface IMessageSenderService
    {
        Task Send(string message);
    }
}
