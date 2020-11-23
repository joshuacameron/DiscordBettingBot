namespace DiscordBettingBot.Service.Dtos
{
    public class Bet
    {
        public string BetterName { get; set; }
        public string MatchName { get; set; }
        public decimal Amount { get; set; }
        public bool WonBet { get; set; }
    }
}
