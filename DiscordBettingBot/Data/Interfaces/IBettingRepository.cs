namespace DiscordBettingBot.Data.Interfaces
{
    public interface IBettingRepository
    {
        void AddMatch(string matchName, string[] team1, string[] team2);
    }
}
