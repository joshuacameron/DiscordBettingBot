using System.Collections.Generic;
using DiscordBettingBot.Service.Enumerations;

namespace DiscordBettingBot.Data.Models
{
    public class Match
    {
        public long Id { get; set; }
        public long TournamentId { get; set; }
        public string Name { get; set; }
        public int? WinningTeamNumber { get; set; }
        public List<Player> Team1 { get; set; }
        public List<Player> Team2 { get; set; }
        public MatchStatus Status { get; set; }
    }
}
