using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBettingBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using DiscordBettingBot.Common.Data;
using DiscordBettingBot.Common.Data.Interfaces;
using DiscordBettingBot.Common.Service;
using DiscordBettingBot.Common.Service.Interfaces;
using DiscordBettingBot.Common.Settings;
using Microsoft.Data.Sqlite;

namespace DiscordBettingBot
{
    public class Program
    {
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private IConfiguration _config;

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _config = BuildConfig();

            var services = ConfigureServices();
            services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {



            var bettingSettings = new BettingSQLiteSettings
            {
                ConnectionString = _config["BettingSQLiteSettings:ConnectionString"]
            };

            var sqliteConnection = new SqliteConnection(bettingSettings.ConnectionString); ;
            sqliteConnection.Open();
            IBettingRepository bettingRepo = new BettingSQLiteRepository(sqliteConnection, false);

            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                // Logging
                .AddLogging()
                .AddSingleton<LogService>()
                .AddSingleton(bettingRepo)
                .AddSingleton<IBettingService, BettingService>()
                // Extra
                .AddSingleton(_config)
                // Add additional services here...
                .BuildServiceProvider();
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }
    }
}
