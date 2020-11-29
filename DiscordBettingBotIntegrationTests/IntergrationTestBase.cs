using Autofac;
using AutoMapper;
using DiscordBettingBot.Common.Common.Startup;
using DiscordBettingBot.Common.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;

namespace DiscordBettingBotIntegrationTests
{
    public class IntegrationTestBase
    {
        protected readonly IContainer Container;

        protected IntegrationTestBase()
        {
            var configuration = GetConfiguration();

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterModule(new CommonModule(configuration));

            var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile(new MappingProfile()); });

            containerBuilder.RegisterInstance(mappingConfig.CreateMapper());

            containerBuilder.RegisterGeneric(typeof(NullLogger<>)).As(typeof(ILogger<>));

            Container = containerBuilder.Build();
        }

        private static IConfiguration GetConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();

            var currentDirectory = Directory.GetCurrentDirectory();

            return configurationBuilder.SetBasePath(currentDirectory)
                .AddJsonFile("appsettings.json", false, true)
                .Build();
        }
    }
}
