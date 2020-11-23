using DiscordBettingBot.Service.Dtos;

namespace DiscordBettingBot.Service.Interfaces
{
    public interface IBettingService
    {
        void AddMatch(string tournamentName, string matchName, string[] team1, string[] team2);
        void StartMatch(string tournamentName, string matchName);
        void RemoveMatch(string tournamentName, string matchName);
        MatchResultDto DeclareMatchWinner(string tournamentName, string matchName, int teamNumber);
        decimal GetBalance(string tournamentName, string betterName);
        MatchDto[] GetAvailableMatches(string tournamentName);
        AddBetResponse AddBet(string tournamentName, string betterName, string matchName, decimal betAmount);
        LeaderboardDto GetLeaderBoard(string tournamentName);
        Better GetBetterInfo(string tournamentName, string betterName);
        void StartNewTournament(string tournamentName);
    }
}
