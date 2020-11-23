using DiscordBettingBot.Data.Interfaces;
using DiscordBettingBot.Service.Models;
using System;

namespace DiscordBettingBot.Data
{
    public class BettingSQLiteRepository : IBettingRepository
    {
        public bool DoesTournamentExist(string tournamentName)
        {
            throw new NotImplementedException();
        }

        public bool DoesMatchExist(string tournamentName, string matchName)
        {
            throw new NotImplementedException();
        }

        public bool DoesBetterExist(string tournamentName, string betterName)
        {
            throw new NotImplementedException();
        }

        public void AddMatch(string tournamentName, string matchName, string[] team1, string[] team2)
        {
            throw new NotImplementedException();
        }

        public void StartMatch(string tournamentName, string matchName)
        {
            throw new NotImplementedException();
        }

        public void RemoveMatch(string tournamentName, string matchName)
        {
            throw new NotImplementedException();
        }

        public void DeclareMatchWinner(string tournamentName, string matchName, int teamNumber)
        {
            throw new NotImplementedException();
        }

        public MatchResult GetMatchResult(string tournamentName, string matchName)
        {
            throw new NotImplementedException();
        }

        public decimal GetBalance(string tournamentName, string betterName)
        {
            throw new NotImplementedException();
        }

        public Match[] GetAvailableMatches(string tournamentName)
        {
            throw new NotImplementedException();
        }

        public void AddBet(string tournamentName, string betterName, string matchName, decimal betAmount)
        {
            throw new NotImplementedException();
        }

        public Leaderboard GetLeaderBoard(string tournamentName)
        {
            throw new NotImplementedException();
        }

        public Better GetBetterInfo(string tournamentName, string betterName)
        {
            throw new NotImplementedException();
        }

        public void StartNewTournament(string tournamentName)
        {
            throw new NotImplementedException();
        }

        public Match GetMatch(string tournamentName, string matchName)
        {
            throw new NotImplementedException();
        }
    }
}
