namespace DiscordBettingBot.Data.Models
{
    public class Bet
    {
        public long Id { get; set; }
        public long BetterId { get; set; }
        public long MatchId { get; set; }
        public decimal Amount { get; set; }
        public bool? Won { get; set; }
        public int TeamNumber { get; set; }
    }
}
