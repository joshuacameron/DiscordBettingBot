using System.Collections.Generic;
using System.Linq;

namespace DiscordBettingBot.Common.Data.Models
{
    public class Better
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public IEnumerable<Bet> Bets { get; set; }
        
        public int WonBetsCount => Bets.Count(x => x.Won == true);
        public int LostBetsCount => Bets.Count(x => x.Won == false);
        public int OutstandingBetsCount => Bets.Count(x => x.Won == null);
    }
}
