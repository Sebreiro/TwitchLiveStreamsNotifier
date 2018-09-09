using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchLiveStreamsNotifier.MessageSender.Config;

namespace TwitchLiveStreamsNotifier.MessageSender
{
    public class MessageSenderService : IMessageSenderService
    {
        readonly ILogger _logger;
        readonly MessageSenderConfig _config;

        public MessageSenderService(ILogger<MessageSenderService> logger, IOptionsSnapshot<MessageSenderConfig> config)
        {
            _logger = logger;
            _config = config.Value;
            CheckConfig(_config);

        }

        public async Task Send(string message)
        {
            if (message == null)
            {
                _logger.LogError("Message is null");
                return;
            }
            var requestUrl = _config.Url;
            var client = new HttpClient();

            var httpContent = new StringContent($"{{message:\"{message}\"}}", Encoding.UTF8, "application/json");
            try
            {
                var result = await client.PostAsync(requestUrl, httpContent);
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    _logger.LogError($"{result}");

            }
            catch (Exception ex) when (ex.InnerException is SocketException)
            {
                _logger.LogError($"RequestUrl: {requestUrl};{Environment.NewLine}RequestMessage: {message}{Environment.NewLine}Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        private bool CheckConfig(MessageSenderConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.Url))
                throw new InvalidOperationException("MessageSenderConfig Url is missing");

            return true;
        }
    }
}
