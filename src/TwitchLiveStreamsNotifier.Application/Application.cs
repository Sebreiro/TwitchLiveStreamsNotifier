using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TwitchLiveStreamsNotifier.Application.Config;
using TwitchLiveStreamsNotifier.Application.Factory;
using TwitchLiveStreamsNotifier.MessageSender;
using TwitchLiveStreamsNotifier.Services.Schedule;

namespace TwitchLiveStreamsNotifier.Application
{
    public class Application
    {
        private readonly ILogger _logger;
        private readonly JobConfig _jobConfig;
        private readonly IMessageJobFactory _messageJobFactory;
        private readonly IScheduleService _scheduleService;
        private readonly IMessageSenderService _messageSender;

        public Application(
            ILogger<Application> logger, 
            IMessageJobFactory messageJobFactory, 
            IScheduleService scheduleService, 
            IMessageSenderService messageSender, 
            IOptions<JobConfig> jobConfig)
        {
            _logger = logger;
            _messageJobFactory = messageJobFactory;
            _scheduleService = scheduleService;
            _messageSender = messageSender;
            _jobConfig = jobConfig.Value;
        }

        public void Start()
        {
            _logger.LogInformation("Starting Twitch Live Streams Notifier");

            _scheduleService.AddRecurrentTask(Job, GetRepeatTime(), "Live Streams Data");
        }
        private async void Job()
        {
            try
            {
                var messageJob = _messageJobFactory.GetStreamsMessageJob();

                var messages = await messageJob.Invoke();
                if (messages == null)
                    return;

                foreach (var message in messages)
                {
                    if (string.IsNullOrWhiteSpace(message)) continue;

                    _logger.LogInformation($"LiveStreamsMessage: {message}");
                    await _messageSender.Send(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Unhandled Exception; {ex}");
            }
        }

        private int GetRepeatTime()
        {
            var requestInterval = _jobConfig.RequestInterval;
            _logger.LogDebug($"Twitch Api Request interval is {requestInterval} min");
            return requestInterval;
        }
    }
}
