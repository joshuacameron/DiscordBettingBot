using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBettingBot.Data.Models;
using DiscordBettingBot.Service.Enumerations;

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

        List<Bet> GetBetsByMatchId(long matchId);
        void AddToBetterAmounts(List<long> betterIds, List<decimal> amounts);
        void UpdateBets(List<Bet> bets);
        void AddBet(Bet bet);

        MatchResult GetMatchResult(long matchId);

        Better GetBetterByName(long tournamentId, string betterName);
        List<Better> GetBetterByTournamentId(long tournamentId);
    }
}
