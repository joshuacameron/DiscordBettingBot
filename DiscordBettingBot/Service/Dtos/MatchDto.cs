using DiscordBettingBot.Service.Enumerations;

namespace DiscordBettingBot.Service.Dtos
{
    public class MatchDto
    {
        public string Name { get; set; }
        public string[] Team1 { get; set; }
        public string[] Team2 { get; set; }
        public MatchStatus Status { get; set; }
    }
}
