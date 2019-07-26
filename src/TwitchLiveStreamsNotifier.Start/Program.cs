using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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

            var configuration = OptionsConfigurator.Configure(serviceCollection);

            LoggingConfiguration.Configure(serviceCollection, configuration);

            var serviceProvider = ContainerConfigurator.Configure(serviceCollection);

            var application = serviceProvider.GetService<Application.Application>();

            application.Start();

            await Task.Delay(-1, cts.Token);

            Console.WriteLine("Closing application");
            return 0;
        }
    }
}
