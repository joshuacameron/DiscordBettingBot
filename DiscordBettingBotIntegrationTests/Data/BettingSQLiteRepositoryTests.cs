using Autofac;
using DiscordBettingBot.Data.Interfaces;

namespace DiscordBettingBotIntegrationTests.Data
{
    public class BettingSQLiteRepositoryTests : IntegrationTestBase
    {
        private IBettingRepository GetService() => Container.Resolve<IBettingRepository>();
    }
}
