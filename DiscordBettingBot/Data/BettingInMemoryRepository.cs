using DiscordBettingBot.Data.Interfaces;
using System;

namespace DiscordBettingBot.Data
{
    public class BettingInMemoryRepository : IBettingRepository
    {
        public void AddMatch(string matchName, string[] team1, string[] team2)
        {
            throw new NotImplementedException();
        }
    }
}
