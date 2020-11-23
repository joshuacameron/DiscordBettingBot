namespace DiscordBettingBot.Service.Models
{
    public class MatchResult
    {
        public Better[] Winners { get; set; }
        public Better[] Losers { get; set; }
    }
}
