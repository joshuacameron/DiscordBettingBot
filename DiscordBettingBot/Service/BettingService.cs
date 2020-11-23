using DiscordBettingBot.Data.Interfaces;
using DiscordBettingBot.Service.Dtos;
using DiscordBettingBot.Service.Interfaces;
using System;

namespace DiscordBettingBot.Service
{
    public class BettingService : IBettingService
    {
        private readonly IBettingRepository _bettingRepository;

        public BettingService(IBettingRepository bettingRepository)
        {
            _bettingRepository = bettingRepository;
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

        public MatchResultDto DeclareMatchWinner(string tournamentName, string matchName, int teamNumber)
        {
            throw new NotImplementedException();
        }

        public decimal GetBalance(string tournamentName, string betterName)
        {
            throw new NotImplementedException();
        }

        public MatchDto[] GetAvailableMatches(string tournamentName)
        {
            throw new NotImplementedException();
        }

        public AddBetResponse AddBet(string tournamentName, string betterName, string matchName, decimal betAmount)
        {
            throw new NotImplementedException();
        }

        public LeaderboardDto GetLeaderBoard(string tournamentName)
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
    }
}
