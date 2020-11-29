using DiscordBettingBot.Data.Models;
using DiscordBettingBot.Service.Enumerations;
using System.Collections.Generic;

namespace DiscordBettingBot.Data.Interfaces
{
    public interface IBettingRepository
    {
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();

        Tournament GetTournamentByName(string tournamentName);
        void InsertTournament(string tournamentName);

        long InsertMatch(Match match);
        Match GetMatchByName(long tournamentId, string matchName);
        void UpdateMatchStatus(long matchId, MatchStatus matchStatus);
        void UpdateMatchWinningTeamNumber(long matchId, int winningTeamNumber);
        void DeleteMatch(long matchId);
        List<Match> GetMatchesByTournamentId(long tournamentId);

        void InsertPlayers(List<Player> players);
        void DeletePlayerByPlayerIds(List<long> playerIds);
        List<Player> GetPlayerByMatchId(long matchId);

        List<Bet> GetBetsByMatchId(long matchId);
        void AddToBetterAmounts(List<long> betterIds, List<decimal> amounts);
        void UpdateBets(List<Bet> bets);
        void AddBet(Bet bet);
        void DeleteBetsById(List<long> betIds);

        MatchResult GetMatchResult(long matchId);
        void InsertBetter(long tournamentId, string betterName, decimal initialBalance);
        Better GetBetterByName(long tournamentId, string betterName);
        List<Better> GetBetterByTournamentId(long tournamentId);
    }
}
