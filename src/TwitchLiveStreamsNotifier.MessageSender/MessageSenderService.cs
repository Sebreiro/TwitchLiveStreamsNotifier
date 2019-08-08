using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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

            var fullMessage = JsonConvert.SerializeObject(new {channelName = _config.ChannelName, message});

            var httpContent = new StringContent(fullMessage, Encoding.UTF8, "application/json");
            
            try
            {
                var result = await client.PostAsync(requestUrl, httpContent);

                if (!result.IsSuccessStatusCode)
                {
                    var responseContent = await result.Content.ReadAsStringAsync();
                    _logger.LogError("RequestUrl: {requestUrl}; " +
                                     "RequestMessage: {requestMessage}; " +
                                     "StatusCode {statusCode}; " +
                                     "Reason {reasonPhrase}; Response {responseContent}",
                        requestUrl, message, result.StatusCode, result.ReasonPhrase, responseContent);
                }
            }
            catch (Exception ex) when (ex.InnerException is SocketException)
            {

                _logger.LogError(ex, $"RequestUrl: {requestUrl};{Environment.NewLine}RequestMessage: {message}");
            }
        }

        private bool CheckConfig(MessageSenderConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.Url))
                throw new InvalidOperationException("MessageSenderConfig Url is missing");
            
            if (string.IsNullOrWhiteSpace(config.ChannelName))
                throw new InvalidOperationException("MessageSenderConfig ChannelName is missing");

            return true;
        }
    }
}
