namespace DiscordBettingBot.Service.Models
{
    public class Better
    {
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public Bet[] Bets { get; set; }
    }
}
