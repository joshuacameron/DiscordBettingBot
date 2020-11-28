using DiscordBettingBot.Data.Interfaces;
using DiscordBettingBot.Service.Enumerations;
using DiscordBettingBot.Service.Exceptions;
using DiscordBettingBot.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.Internal;
using DiscordBettingBot.Data.Models;

namespace DiscordBettingBot.Service
{
    //TODO: Verify correct state at all times
    //TODO: Validate VerifyValidBetAmount
    public class BettingService : IBettingService
    {
        private readonly IBettingRepository _bettingRepository;

        public BettingService(IBettingRepository bettingRepository)
        {
            _bettingRepository = bettingRepository;
        }

        public void AddMatch(string tournamentName, string matchName, string[] team1, string[] team2)
        {
            VerifyValidTournamentName(tournamentName);
            VerifyValidMatchName(matchName);
            team1.ForAll(VerifyValidPlayerName);
            team2.ForAll(VerifyValidPlayerName);

            try
            {
                _bettingRepository.BeginTransaction();

                var tournament = _bettingRepository.GetTournamentByName(tournamentName);

                if (tournament == null)
                {
                    throw new TournamentDoesNotExistException(tournamentName);
                }

                if (_bettingRepository.GetMatchByName(tournament.Id, matchName) != null)
                {
                    throw new MatchAlreadyExistsException(matchName);
                }

                var match = new Match
                {
                    TournamentId = tournament.Id,
                    Name = matchName,
                    Status = MatchStatus.WaitingToStart
                };

                var matchId = _bettingRepository.InsertMatch(match);

                var players = new List<Player>();
                players.AddRange(team1.Select(x => new Player {MatchId = matchId, Name = x, TeamNumber = 1}));
                players.AddRange(team2.Select(x => new Player {MatchId = matchId, Name = x, TeamNumber = 2}));

                _bettingRepository.InsertPlayers(players);
 
                _bettingRepository.CommitTransaction();
            }
            catch
            {
                _bettingRepository.RollbackTransaction();
            }
        }

        public void StartMatch(string tournamentName, string matchName)
        {
            VerifyValidTournamentName(tournamentName);
            VerifyValidMatchName(matchName);
            
            try
            {
                _bettingRepository.BeginTransaction();

                var tournament = _bettingRepository.GetTournamentByName(tournamentName);

                if (tournament == null)
                {
                    throw new TournamentDoesNotExistException(tournamentName);
                }



                _bettingRepository.CommitTransaction();
            }
            catch
            {
                _bettingRepository.RollbackTransaction();
            }


            VerifyTournamentExists(tournamentName);
            VerifyMatchExists(tournamentName, matchName);
            VerifyMatchWaitingToStart(tournamentName, matchName);

            _bettingRepository.StartMatch(tournamentName, matchName);
        }

        public void RemoveMatch(string tournamentName, string matchName)
        {
            VerifyValidTournamentName(tournamentName);
            VerifyValidMatchName(matchName);

            VerifyTournamentExists(tournamentName);
            VerifyMatchExists(tournamentName, matchName);

            _bettingRepository.DeleteMatch(tournamentName, matchName);
        }

        public MatchResult DeclareMatchWinner(string tournamentName, string matchName, int teamNumber)
        {
            VerifyValidTournamentName(tournamentName);
            VerifyValidMatchName(matchName);
            VerifyValidTeamNumber(teamNumber);

            VerifyTournamentExists(tournamentName);
            VerifyMatchExists(tournamentName, matchName);
            VerifyMatchRunning(tournamentName, matchName);

            _bettingRepository.DeclareMatchWinner(tournamentName, matchName, teamNumber);
            return _bettingRepository.GetMatchResult(tournamentName, matchName);
        }

        public decimal GetBalance(string tournamentName, string betterName)
        {
            VerifyValidTournamentName(tournamentName);
            VerifyValidBetterName(betterName);

            VerifyTournamentExists(tournamentName);
            VerifyBetterExists(tournamentName, betterName);

            return _bettingRepository.GetBalance(tournamentName, betterName);
        }

        public IEnumerable<Match> GetMatches(string tournamentName)
        {
            VerifyValidTournamentName(tournamentName);

            VerifyTournamentExists(tournamentName);

            return _bettingRepository.GetMatches(tournamentName);
        }

        public void AddBet(string tournamentName, string betterName, string matchName, decimal betAmount, int teamNumber)
        {
            VerifyValidTournamentName(tournamentName);
            VerifyValidBetterName(betterName);
            VerifyValidMatchName(matchName);
            VerifyValidBetAmount(betAmount);

            VerifyTournamentExists(tournamentName);
            VerifyMatchExists(tournamentName, matchName);
            VerifyMatchWaitingToStart(tournamentName, matchName);

            _bettingRepository.AddBet(tournamentName, betterName, matchName, betAmount, teamNumber);
        }

        public IEnumerable<Better> GetLeaderBoard(string tournamentName)
        {
            VerifyValidTournamentName(tournamentName);

            VerifyTournamentExists(tournamentName);

            return _bettingRepository.GetLeaderBoard(tournamentName);
        }

        public Better GetBetterInfo(string tournamentName, string betterName)
        {
            VerifyValidTournamentName(tournamentName);
            VerifyValidBetterName(betterName);

            VerifyTournamentExists(tournamentName);
            VerifyBetterExists(tournamentName, betterName);

            return _bettingRepository.GetBetterInfo(tournamentName, betterName);
        }

        public void StartNewTournament(string tournamentName)
        {
            VerifyValidTournamentName(tournamentName);

            VerifyTournamentDoesNotExist(tournamentName);

            _bettingRepository.StartNewTournament(tournamentName);
        }

        #region Helpers

        private void VerifyValidTournamentName(string tournamentName)
        {
            if (string.IsNullOrEmpty(tournamentName) || tournamentName.Length > 254)
            {
                throw new InvalidTournamentNameException(tournamentName);
            }
        }

        private void VerifyValidMatchName(string matchName)
        {
            if (string.IsNullOrEmpty(matchName) || matchName.Length > 254)
            {
                throw new InvalidMatchNameException(matchName);
            }
        }

        private void VerifyValidPlayerName(string playerName)
        {
            if (string.IsNullOrEmpty(playerName) || playerName.Length > 254)
            {
                throw new InvalidPlayerNameException(playerName);
            }
        }

        private void VerifyValidBetterName(string betterName)
        {
            if (string.IsNullOrEmpty(betterName) || betterName.Length > 254)
            {
                throw new InvalidBetterNameException(betterName);
            }
        }

        private Tournament VerifyTournamentExists(string tournamentName)
        {
            var tournament = _bettingRepository.GetTournamentByName(tournamentName);

            if (tournament == null)
            {
                throw new TournamentDoesNotExistException(tournamentName);
            }

            return tournament;
        }

        private void VerifyTournamentDoesNotExist(string tournamentName)
        {
            if (_bettingRepository.GetTournamentByName(tournamentName) != null)
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
            var match = _bettingRepository.GetMatch(tournamentName, matchName);

            if (match == null)
            {
                throw new MatchDoesNotExistsException(matchName);
            }

            if (match.Status != MatchStatus.WaitingToStart)
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

        private static void VerifyValidBetAmount(decimal betAmount)
        {
            if (IsLessThanOrEqualToTwoDecimalPlaces(betAmount) || betAmount < 0.01m)
            {
                throw new InvalidBetAmountException(betAmount);
            }
        }

        private static void VerifyValidTeamNumber(int teamNumber)
        {
            if (teamNumber != 1 && teamNumber != 2)
            {
                throw new InvalidTeamNumberException(teamNumber);
            }
        }

        private static bool IsLessThanOrEqualToTwoDecimalPlaces(decimal dec)
        {
            decimal value = dec * 100;
            return value != Math.Floor(value);
        }

        #endregion
    }
}
