using System.Collections.Generic;

namespace DiscordBettingBot.Data.Models
{
    public class MatchResult
    {
        public int? WinningTeamNumber { get; set; }
        public IEnumerable<Bet> WinningBets { get; set; }
        public IEnumerable<Bet> LosingBets { get; set; }
    }
}
