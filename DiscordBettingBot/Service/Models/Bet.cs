namespace DiscordBettingBot.Service.Models
{
    public class Bet
    {
        internal long Id { get; set; }
        internal long BetterId { get; set; }
        public string BetterName { get; set; }
        public string MatchName { get; set; }
        public decimal Amount { get; set; }
        public bool Won { get; set; }
        public int TeamNumber { get; set; }
    }
}
