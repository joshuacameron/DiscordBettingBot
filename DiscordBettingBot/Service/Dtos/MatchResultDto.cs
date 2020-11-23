namespace DiscordBettingBot.Service.Dtos
{
    public class MatchResultDto
    {
        public Better[] Winners { get; set; }
        public Better[] Losers { get; set; }
    }
}
