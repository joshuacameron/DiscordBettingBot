namespace DiscordBettingBot.Data.Models
{
    public class Player
    {
        internal long Id { get; set; }
        public string Name { get; set; }
        internal long MatchId { get; set; }
        public int TeamNumber { get; set; }
    }
}
