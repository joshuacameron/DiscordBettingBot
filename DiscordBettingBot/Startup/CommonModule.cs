using Autofac;
using DiscordBettingBot.Common.Data;
using DiscordBettingBot.Common.Data.Interfaces;
using DiscordBettingBot.Common.Settings;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Reflection;
using Module = Autofac.Module;

namespace DiscordBettingBot.Common.Common.Startup
{
    public class CommonModule : Module
    {
        private readonly IConfiguration _configuration;

        public CommonModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.GetInterfaces().Any())
                .AsImplementedInterfaces();
            
            builder.RegisterInstance(_configuration.GetSection("BettingSQLiteSettings").Get<BettingSQLiteSettings>());

            builder.Register(c =>
            {
                var settings = c.Resolve<BettingSQLiteSettings>();
                var sqliteConnection = new SqliteConnection(settings.ConnectionString);;
                sqliteConnection.Open();
                return new BettingSQLiteRepository(sqliteConnection, true);
            }).As<IBettingRepository>();
        }
    }
}
