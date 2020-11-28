namespace DiscordBettingBot.Data.Models
{
    public class Player
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long MatchId { get; set; }
        public int TeamNumber { get; set; }
    }
}
