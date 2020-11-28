using System.Collections.Generic;

namespace DiscordBettingBot.Data.Models
{
    public class Tournament
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Match> Matches { get; set; }
    }
}
