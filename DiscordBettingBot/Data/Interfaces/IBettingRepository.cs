using System.Collections.Generic;
using DiscordBettingBot.Data.Models;

namespace DiscordBettingBot.Data.Interfaces
{
    public interface IBettingRepository
    {
        bool DoesTournamentExist(string tournamentName);
        bool DoesMatchExist(string tournamentName, string matchName);
        bool DoesBetterExist(string tournamentName, string betterName);
        void AddMatch(string tournamentName, string matchName, IEnumerable<string> team1, IEnumerable<string> team2);
        void StartMatch(string tournamentName, string matchName);
        void RemoveMatch(string tournamentName, string matchName);
        void DeclareMatchWinner(string tournamentName, string matchName, int teamNumber);
        MatchResult GetMatchResult(string tournamentName, string matchName);
        decimal GetBalance(string tournamentName, string betterName);
        IEnumerable<Match> GetMatches(string tournamentName);
        void AddBet(string tournamentName, string betterName, string matchName, decimal betAmount, int teamNumber);
        IEnumerable<Better> GetLeaderBoard(string tournamentName);
        Better GetBetterInfo(string tournamentName, string betterName);
        void StartNewTournament(string tournamentName);
        Match GetMatch(string tournamentName, string matchName);
    }
}
