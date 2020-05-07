using System;
using FluentScheduler;
using Microsoft.Extensions.Logging;

namespace TwitchLiveStreamsNotifier.Services.Schedule
{
    public class ScheduleService : IScheduleService
    {
        private readonly ILogger<ScheduleService> _logger;

        public ScheduleService(ILogger<ScheduleService> logger)
        {
            _logger = logger;

            InitializeJobManager();
        }

        private void InitializeJobManager()
        {
            JobManager.JobStart += (info) => _logger.LogTrace($"JobStart start; Job Name:{info.Name}");
            JobManager.JobEnd += info => _logger.LogTrace($"job end; Job name: {info.Name}");
            JobManager.JobException += info => _logger.LogError($"Job Exception; Job name: {info.Name}. Exception: {info.Exception}");

            //JobManager.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="repeatTime">in minutes</param>
        /// <param name="jobName"></param>
        public void AddRecurrentTask(Action job, int repeatTime, string jobName)
        {
            if (job == null)
                throw new InvalidOperationException("Job is null");

            if (repeatTime == 0)
                throw new InvalidOperationException($"{nameof(repeatTime)} should be more than 0");


            JobManager.AddJob(job, (s) => s.WithName(jobName).ToRunNow().AndEvery(repeatTime).Minutes());
        }

        public void AddOneTimeTask(Action job, string jobName)
        {
            throw new NotImplementedException();
        }
    }
}
