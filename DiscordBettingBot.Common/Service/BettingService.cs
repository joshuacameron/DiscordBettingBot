using AutoMapper.Internal;
using DiscordBettingBot.Common.Data.Interfaces;
using DiscordBettingBot.Common.Data.Models;
using DiscordBettingBot.Common.Service.Enumerations;
using DiscordBettingBot.Common.Service.Exceptions;
using DiscordBettingBot.Common.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBettingBot.Common.Service
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
            VerifyValidTournamentName(tournamentName);
            VerifyValidMatchName(matchName);
            team1.ForAll(VerifyValidPlayerName);
            team2.ForAll(VerifyValidPlayerName);

            try
            {
                _bettingRepository.BeginTransaction();

                var tournament = GetTournamentByName(tournamentName);

                var matchAlreadyExist = _bettingRepository.GetMatchByName(tournament.Id, matchName) != null;

                if (matchAlreadyExist)
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
                players.AddRange(team1.Select(x => new Player { MatchId = matchId, Name = x, TeamNumber = 1 }));
                players.AddRange(team2.Select(x => new Player { MatchId = matchId, Name = x, TeamNumber = 2 }));

                _bettingRepository.InsertPlayers(players);

                _bettingRepository.CommitTransaction();
            }
            catch
            {
                _bettingRepository.RollbackTransaction();
                throw;
            }
        }

        public void StartMatch(string tournamentName, string matchName)
        {
            VerifyValidTournamentName(tournamentName);
            VerifyValidMatchName(matchName);

            try
            {
                _bettingRepository.BeginTransaction();

                var tournament = GetTournamentByName(tournamentName);

                var match = _bettingRepository.GetMatchByName(tournament.Id, matchName);

                if (match == null)
                {
                    throw new MatchDoesNotExistsException(matchName);
                }

                if (match.Status != MatchStatus.WaitingToStart)
                {
                    throw new MatchNotWaitingToStartException(matchName);
                }

                _bettingRepository.UpdateMatchStatus(match.Id, MatchStatus.Running);

                _bettingRepository.CommitTransaction();
            }
            catch
            {
                _bettingRepository.RollbackTransaction();
                throw;
            }
        }

        public void RemoveMatch(string tournamentName, string matchName)
        {
            VerifyValidTournamentName(tournamentName);
            VerifyValidMatchName(matchName);

            try
            {
                _bettingRepository.BeginTransaction();

                var tournament = GetTournamentByName(tournamentName);

                var match = GetMatchByName(tournament.Id, matchName);

                var bets = _bettingRepository.GetBetsByMatchId(match.Id);

                _bettingRepository.AddToBetterAmounts(
                    bets.Select(x => x.BetterId).ToList(),
                    bets.Select(x => x.Amount).ToList()
                );

                _bettingRepository.DeleteBetsById(bets.Select(x => x.Id).ToList());

                var players = new List<Player>();
                players.AddRange(match.Team1);
                players.AddRange(match.Team2);
                _bettingRepository.DeletePlayerByPlayerIds(players.Select(x => x.Id).ToList());

                _bettingRepository.DeleteMatch(match.Id);

                _bettingRepository.CommitTransaction();
            }
            catch
            {
                _bettingRepository.RollbackTransaction();
                throw;
            }
        }

        public MatchResult DeclareMatchWinner(string tournamentName, string matchName, int teamNumber)
        {
            VerifyValidTournamentName(tournamentName);
            VerifyValidMatchName(matchName);
            VerifyValidTeamNumber(teamNumber);

            try
            {
                _bettingRepository.BeginTransaction();

                var tournament = GetTournamentByName(tournamentName);

                var match = GetMatchByName(tournament.Id, matchName);

                _bettingRepository.UpdateMatchStatus(match.Id, MatchStatus.Finished);
                _bettingRepository.UpdateMatchWinningTeamNumber(match.Id, teamNumber);

                var bets = _bettingRepository.GetBetsByMatchId(match.Id);

                var payouts = new List<(long BetterId, decimal Amount)>();

                foreach (var bet in bets)
                {
                    bet.Won = bet.TeamNumber == teamNumber;

                    if (bet.Won == true)
                    {
                        payouts.Add((bet.BetterId, bet.Amount * 2));
                    }
                }

                _bettingRepository.UpdateBets(bets);

                _bettingRepository.AddToBetterAmounts(
                    payouts.Select(x => x.BetterId).ToList(),
                    payouts.Select(x => x.Amount).ToList()
                );

                var matchResult = _bettingRepository.GetMatchResult(match.Id);

                _bettingRepository.CommitTransaction();

                return matchResult;
            }
            catch
            {
                _bettingRepository.RollbackTransaction();
                throw;
            }
        }

        public decimal GetBalance(string tournamentName, string betterName)
        {
            VerifyValidTournamentName(tournamentName);
            VerifyValidBetterName(betterName);
            
            return GetBetterInfo(tournamentName, betterName).Balance;
        }

        public IEnumerable<Match> GetMatches(string tournamentName)
        {
            VerifyValidTournamentName(tournamentName);

            try
            {
                _bettingRepository.BeginTransaction();

                var tournament = GetTournamentByName(tournamentName);

                var matches = _bettingRepository.GetMatchesByTournamentId(tournament.Id);

                _bettingRepository.CommitTransaction();

                return matches;
            }
            catch
            {
                _bettingRepository.RollbackTransaction();
                throw;
            }
        }

        public void AddBet(string tournamentName, string betterName, string matchName, decimal betAmount, int teamNumber)
        {
            VerifyValidTournamentName(tournamentName);
            VerifyValidBetterName(betterName);
            VerifyValidMatchName(matchName);
            VerifyValidBetAmount(betAmount);
            VerifyValidTeamNumber(teamNumber);

            try
            {
                _bettingRepository.BeginTransaction();

                var tournament = GetTournamentByName(tournamentName);

                var match = GetMatchByName(tournament.Id, matchName);

                if (match.Status != MatchStatus.WaitingToStart)
                {
                    throw new MatchNotWaitingToStartException(matchName);
                }

                var better = _bettingRepository.GetBetterByName(tournament.Id, betterName);

                if (better == null)
                {
                    throw new BetterDoesNotExistException(betterName);
                }

                var bet = new Bet
                {
                    Amount = betAmount,
                    MatchId = match.Id,
                    TeamNumber = teamNumber,
                    BetterId = better.Id
                };

                _bettingRepository.AddBet(bet);

                _bettingRepository.AddToBetterAmounts(new List<long> {better.Id}, new List<decimal> {-1 * betAmount});

                _bettingRepository.CommitTransaction();
            }
            catch
            {
                _bettingRepository.RollbackTransaction();
                throw;
            }
        }

        public IEnumerable<Better> GetLeaderBoard(string tournamentName)
        {
            VerifyValidTournamentName(tournamentName);

            try
            {
                _bettingRepository.BeginTransaction();

                var tournament = GetTournamentByName(tournamentName);

                var betters = _bettingRepository.GetBetterByTournamentId(tournament.Id).OrderByDescending(x => x.Balance);

                _bettingRepository.CommitTransaction();

                return betters;
            }
            catch
            {
                _bettingRepository.RollbackTransaction();
                throw;
            }
        }

        public Better GetBetterInfo(string tournamentName, string betterName)
        {
            VerifyValidTournamentName(tournamentName);
            VerifyValidBetterName(betterName);

            try
            {
                _bettingRepository.BeginTransaction();
                
                var tournament = GetTournamentByName(tournamentName);

                var better = _bettingRepository.GetBetterByName(tournament.Id, betterName);

                if (better == null)
                {
                    throw new BetterDoesNotExistException(betterName);
                }

                _bettingRepository.CommitTransaction();

                return better;
            }
            catch
            {
                _bettingRepository.RollbackTransaction();
                throw;
            }
        }

        public void AddBetter(string tournamentName, string betterName, decimal initialBalance)
        {
            VerifyValidTournamentName(tournamentName);
            VerifyValidBetterName(betterName);
            VerifyValidInitialBalance(initialBalance);

            try
            {
                _bettingRepository.BeginTransaction();

                var tournament = GetTournamentByName(tournamentName);
                _bettingRepository.InsertBetter(tournament.Id, betterName, initialBalance);
                
                _bettingRepository.CommitTransaction();
            }
            catch
            {
                _bettingRepository.RollbackTransaction();
                throw;
            }
        }

        public void StartNewTournament(string tournamentName)
        {
            VerifyValidTournamentName(tournamentName);

            try
            {
                _bettingRepository.BeginTransaction();

                var tournament = _bettingRepository.GetTournamentByName(tournamentName);

                if (tournament != null)
                {
                    throw new TournamentAlreadyExistsException(tournamentName);
                }

                _bettingRepository.InsertTournament(tournamentName);

                _bettingRepository.CommitTransaction();
            }
            catch
            {
                _bettingRepository.RollbackTransaction();
                throw;
            }
        }

        #region Verify Valid Helpers

        private static void VerifyValidTournamentName(string tournamentName)
        {
            if (string.IsNullOrEmpty(tournamentName) || tournamentName.Length > 254)
            {
                throw new InvalidTournamentNameException(tournamentName);
            }
        }

        private static void VerifyValidMatchName(string matchName)
        {
            if (string.IsNullOrEmpty(matchName) || matchName.Length > 254)
            {
                throw new InvalidMatchNameException(matchName);
            }
        }

        private static void VerifyValidPlayerName(string playerName)
        {
            if (string.IsNullOrEmpty(playerName) || playerName.Length > 254)
            {
                throw new InvalidPlayerNameException(playerName);
            }
        }

        private static void VerifyValidBetterName(string betterName)
        {
            if (string.IsNullOrEmpty(betterName) || betterName.Length > 254)
            {
                throw new InvalidBetterNameException(betterName);
            }
        }

        private static void VerifyValidTeamNumber(int teamNumber)
        {
            if (teamNumber != 1 && teamNumber != 2)
            {
                throw new InvalidTeamNumberException(teamNumber);
            }
        }

        private static void VerifyValidBetAmount(decimal betAmount)
        {
            if (IsLessThanOrEqualToTwoDecimalPlaces(betAmount) || betAmount < 0.01m)
            {
                throw new InvalidBetAmountException(betAmount);
            }
        }

        private static bool IsLessThanOrEqualToTwoDecimalPlaces(decimal dec)
        {
            decimal value = dec * 100;
            return value != Math.Floor(value);
        }

        private static void VerifyValidInitialBalance(decimal initialBalance)
        {
            if (initialBalance < 0)
            {
                throw new InvalidInitialAmountException(initialBalance);
            }
        }

        #endregion
        #region Verify Helpers

        private Tournament GetTournamentByName(string tournamentName)
        {
            var tournament = _bettingRepository.GetTournamentByName(tournamentName);

            if (tournament == null)
            {
                throw new TournamentDoesNotExistException(tournamentName);
            }

            return tournament;
        }

        private Match GetMatchByName(long tournamentId, string matchName)
        {
            var match = _bettingRepository.GetMatchByName(tournamentId, matchName);

            if (match == null)
            {
                throw new MatchDoesNotExistsException(matchName);
            }

            return match;
        }

        #endregion
    }
}