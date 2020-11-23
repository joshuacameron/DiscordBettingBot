using DiscordBettingBot.Service.Models;

namespace DiscordBettingBot.Service.Interfaces
{
    public interface IBettingService
    {
        void AddMatch(string tournamentName, string matchName, string[] team1, string[] team2);
        void StartMatch(string tournamentName, string matchName);
        void RemoveMatch(string tournamentName, string matchName);
        MatchResult DeclareMatchWinner(string tournamentName, string matchName, int teamNumber);
        decimal GetBalance(string tournamentName, string betterName);
        Match[] GetAvailableMatches(string tournamentName);
        void AddBet(string tournamentName, string betterName, string matchName, decimal betAmount);
        Leaderboard GetLeaderBoard(string tournamentName);
        Better GetBetterInfo(string tournamentName, string betterName);
        void StartNewTournament(string tournamentName);
    }
}
