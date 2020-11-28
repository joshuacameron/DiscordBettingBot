using System.Collections.Generic;

namespace DiscordBettingBot.Data.Models
{
    public class Better
    {
        internal long Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public IEnumerable<Bet> Bets { get; set; }
    }
}
