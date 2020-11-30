using System.Collections.Generic;

namespace DiscordBettingBot.Common.Data.Models
{
    public class MatchResult
    {
        public long Id { get; set; }
        public string MatchName { get; set; }
        public string[] WinningPlayers { get; set; }
        public int? WinningTeamNumber { get; set; }
        public IEnumerable<Bet> Bets { get; set; }
        public IEnumerable<Better> MatchBetters { get; set; }
    }
}
