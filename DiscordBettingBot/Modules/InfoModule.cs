using Discord.Commands;
using System.Threading.Tasks;

namespace DiscordBettingBot.Modules
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("info")]
        public Task Info()
            => ReplyAsync(
                $"Hello, I am a bot called {Context.Client.CurrentUser.Username} written in Discord.Net 2.2.0\n");
    }
}
