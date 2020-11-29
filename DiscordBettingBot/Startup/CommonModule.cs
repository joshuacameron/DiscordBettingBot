using Autofac;
using DiscordBettingBot.Settings;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Reflection;
using DiscordBettingBot.Data;
using DiscordBettingBot.Data.Interfaces;
using Microsoft.Data.Sqlite;
using Module = Autofac.Module;

namespace DiscordBettingBot.Startup
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
                return new BettingSQLiteRepository(sqliteConnection, true);
            }).As<IBettingRepository>();
        }
    }
}
