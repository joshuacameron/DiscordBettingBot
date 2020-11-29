using DiscordBettingBot.Common.Data.Models;
using System.Collections.Generic;

namespace DiscordBettingBot.Common.Service.Interfaces
{
    public interface IBettingService
    {
        void AddMatch(string tournamentName, string matchName, string[] team1, string[] team2);
        void StartMatch(string tournamentName, string matchName);
        void RemoveMatch(string tournamentName, string matchName);
        MatchResult DeclareMatchWinner(string tournamentName, string matchName, int teamNumber);
        decimal GetBalance(string tournamentName, string betterName);
        IEnumerable<Match> GetMatches(string tournamentName);
        void AddBet(string tournamentName, string betterName, string matchName, decimal betAmount, int teamNumber);
        IEnumerable<Better> GetLeaderBoard(string tournamentName);
        Better GetBetterInfo(string tournamentName, string betterName);
        void AddBetter(string tournamentName, string betterName, decimal initialBalance);
        void StartNewTournament(string tournamentName);
    }
}
