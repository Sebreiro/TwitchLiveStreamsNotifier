using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TwitchLiveStreamsNotifier.Start.Initialization;

namespace TwitchLiveStreamsNotifier.Start
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Starting Application");

            var cts = new CancellationTokenSource();
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                cts.Cancel();
            };

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging((builder) => builder.SetMinimumLevel(LogLevel.Trace));

            OptionsConfigurator.Configure(serviceCollection);

            var serviceProvider = ContainerConfigurator.Configure(serviceCollection);

            NLogConfigurator.Configure(serviceProvider);

            var application = serviceProvider.GetService<Application.Application>();

            application.Start();

            await Task.Delay(-1, cts.Token);
            NLog.LogManager.Shutdown();

            Console.WriteLine("Closing application");
            return 0;
        }
    }
}
