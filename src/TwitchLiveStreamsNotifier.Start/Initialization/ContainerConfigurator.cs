using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using TwitchLiveStreamsNotifier.Application.Factory;
using TwitchLiveStreamsNotifier.Application.Streams;
using TwitchLiveStreamsNotifier.LiveStreamsCache.Data;
using TwitchLiveStreamsNotifier.LiveStreamsCache.Services;
using TwitchLiveStreamsNotifier.MessageSender;
using TwitchLiveStreamsNotifier.Services.Schedule;

namespace TwitchLiveStreamsNotifier.Start.Initialization
{
    public static class ContainerConfigurator
    {
        public static IServiceProvider Configure(IServiceCollection serviceCollection)
        {
            var containerBuilder = new Autofac.ContainerBuilder();
            containerBuilder.Populate(serviceCollection);

            Register(containerBuilder);

             var container = containerBuilder.Build();

            var serviceProvider = new AutofacServiceProvider(container);
            return serviceProvider;
        }

        private static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<ScheduleService>().As<IScheduleService>();
            builder.RegisterType<MessageSenderService>().As<IMessageSenderService>();
            builder.RegisterType<MessageJobFactory>().As<IMessageJobFactory>();
            builder.RegisterType<TwitchStreamsDataAggregator>().As<ITwitchStreamsDataAggregator>();
            builder.RegisterType<LiveStreamsCacheManager>().As<ILiveStreamsCacheManager>();
            builder.RegisterType<LiveStreamCacheRepository>().As<ILiveStreamRepository>();
            builder.RegisterType<Application.Application>().As<Application.Application>();
            
        }
    }
}
