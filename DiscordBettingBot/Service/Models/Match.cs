using DiscordBettingBot.Service.Enumerations;

namespace DiscordBettingBot.Service.Models
{
    public class Match
    {
        public string Name { get; set; }
        public string[] Team1 { get; set; }
        public string[] Team2 { get; set; }
        public MatchStatus Status { get; set; }
    }
}
