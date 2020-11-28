using System.Collections.Generic;
using DiscordBettingBot.Service.Enumerations;

namespace DiscordBettingBot.Service.Models
{
    public class Match
    {
        internal long Id { get; set; }
        public string Name { get; set; }
        public int WinningTeamNumber { get; set; }
        public IEnumerable<string> Team1 { get; set; }
        public IEnumerable<string> Team2 { get; set; }
        public MatchStatus Status { get; set; }
    }
}
