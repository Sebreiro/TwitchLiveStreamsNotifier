using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TwitchLiveStreamsNotifier.Application.Config;
using TwitchLiveStreamsNotifier.LiveStreamsCache.Config;
using TwitchLiveStreamsNotifier.MessageSender.Config;

namespace TwitchLiveStreamsNotifier.Start.Initialization
{
    public static class OptionsConfigurator
    {
        private static IConfigurationRoot Config(string environmentName) => new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Config/appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"Config/appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public static IConfiguration Configure(IServiceCollection serviceCollection)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?.ToLower();
            
            serviceCollection.AddOptions();
            var configurationRoot = Config(environmentName);

            AddConfigParts(serviceCollection, configurationRoot);
            
            return configurationRoot;
        }

        private static void AddConfigParts(IServiceCollection serviceCollection,IConfigurationRoot configurationRoot)
        {
            serviceCollection.Configure<TwitchConfig>(configurationRoot.GetSection("twitchConfig"));
            serviceCollection.Configure<MessageSenderConfig>(configurationRoot.GetSection("messageSender"));
            serviceCollection.Configure<JobConfig>(configurationRoot.GetSection("jobConfig"));
            serviceCollection.Configure<StreamCacheConfig>(configurationRoot.GetSection("streamCacheConfig"));
        }
    }
}
