using DiscordBettingBot.Data.Interfaces;
using DiscordBettingBot.Service.Enumerations;
using DiscordBettingBot.Service.Exceptions;
using DiscordBettingBot.Service.Interfaces;
using DiscordBettingBot.Service.Models;
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
            VerifyTournamentExists(tournamentName);
            VerifyMatchDoesNotExist(tournamentName, matchName);

            _bettingRepository.AddMatch(tournamentName, matchName, team1, team2);
        }

        public void StartMatch(string tournamentName, string matchName)
        {
            VerifyTournamentExists(tournamentName);
            VerifyMatchExists(tournamentName, matchName);
            VerifyMatchWaitingToStart(tournamentName, matchName);

            _bettingRepository.StartMatch(tournamentName, matchName);
        }

        public void RemoveMatch(string tournamentName, string matchName)
        {
            VerifyTournamentExists(tournamentName);
            VerifyMatchExists(tournamentName, matchName);

            _bettingRepository.RemoveMatch(tournamentName, matchName);
        }

        public MatchResult DeclareMatchWinner(string tournamentName, string matchName, int teamNumber)
        {
            VerifyTournamentExists(tournamentName);
            VerifyMatchRunning(tournamentName, matchName);
            VerifyTeamNumber(teamNumber);

            _bettingRepository.DeclareMatchWinner(tournamentName, matchName, teamNumber);
            return _bettingRepository.GetMatchResult(tournamentName, matchName);
        }

        public decimal GetBalance(string tournamentName, string betterName)
        {
            VerifyTournamentExists(tournamentName);
            VerifyBetterExists(tournamentName, betterName);

            return _bettingRepository.GetBalance(tournamentName, betterName);
        }

        public Match[] GetAvailableMatches(string tournamentName)
        {
            VerifyTournamentExists(tournamentName);

            return _bettingRepository.GetAvailableMatches(tournamentName);
        }

        public void AddBet(string tournamentName, string betterName, string matchName, decimal betAmount)
        {
            VerifyTournamentExists(tournamentName);
            VerifyMatchExists(tournamentName, matchName);
            VerifyMatchWaitingToStart(tournamentName, matchName);
            VerifyBet(betAmount);

            _bettingRepository.AddBet(tournamentName, betterName, matchName, betAmount);
        }

        public Leaderboard GetLeaderBoard(string tournamentName)
        {
            VerifyTournamentExists(tournamentName);

            return _bettingRepository.GetLeaderBoard(tournamentName);
        }

        public Better GetBetterInfo(string tournamentName, string betterName)
        {
            VerifyTournamentExists(tournamentName);
            VerifyBetterExists(tournamentName, betterName);

            return _bettingRepository.GetBetterInfo(tournamentName, betterName);
        }

        public void StartNewTournament(string tournamentName)
        {
            VerifyTournamentDoesNotExist(tournamentName);

            _bettingRepository.StartNewTournament(tournamentName);
        }

        #region Helpers

        private void VerifyTournamentExists(string tournamentName)
        {
            if (!_bettingRepository.DoesTournamentExist(tournamentName))
            {
                throw new TournamentDoesNotExistException(tournamentName);
            }
        }

        private void VerifyTournamentDoesNotExist(string tournamentName)
        {
            if (_bettingRepository.DoesTournamentExist(tournamentName))
            {
                throw new TournamentAlreadyExistsException(tournamentName);
            }
        }

        private void VerifyMatchExists(string tournamentName, string matchName)
        {
            if (!_bettingRepository.DoesMatchExist(tournamentName, matchName))
            {
                throw new MatchDoesNotExistsException(matchName);
            }
        }

        private void VerifyMatchDoesNotExist(string tournamentName, string matchName)
        {
            if (_bettingRepository.DoesMatchExist(tournamentName, matchName))
            {
                throw new MatchAlreadyExistsException(matchName);
            }
        }

        private void VerifyMatchWaitingToStart(string tournamentName, string matchName)
        {
            if (_bettingRepository.GetMatch(tournamentName, matchName).Status != MatchStatus.WaitingToStart)
            {
                throw new MatchNotWaitingToStartException(matchName);
            }
        }

        private void VerifyMatchRunning(string tournamentName, string matchName)
        {
            if (_bettingRepository.GetMatch(tournamentName, matchName).Status != MatchStatus.Running)
            {
                throw new MatchNotRunningException(matchName);
            }
        }

        private void VerifyBetterExists(string tournamentName, string betterName)
        {
            if (!_bettingRepository.DoesBetterExist(tournamentName, betterName))
            {
                throw new BetterDoesNotExistException(betterName);
            }
        }

        private static void VerifyBet(decimal betAmount)
        {
            if (IsLessThanOrEqualToTwoDecimalPlaces(betAmount) || betAmount < 0.01m)
            {
                throw new InvalidBetAmountException(betAmount);
            }
        }

        private static void VerifyTeamNumber(int teamNumber)
        {
            if (teamNumber != 1 && teamNumber != 2)
            {
                throw new InvalidTeamNumber(teamNumber);
            }
        }

        private static bool IsLessThanOrEqualToTwoDecimalPlaces(decimal dec)
        {
            decimal value = dec * 100;
            return value == Math.Floor(value);
        }

        #endregion
    }
}
