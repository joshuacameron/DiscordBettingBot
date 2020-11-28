using Dapper;
using DiscordBettingBot.Data.Interfaces;
using DiscordBettingBot.Data.Models;
using DiscordBettingBot.Service.Enumerations;
using DiscordBettingBot.Service.Exceptions;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBettingBot.Data
{
    public class BettingSQLiteRepository : IBettingRepository
    {
        private readonly SqliteConnection _sqliteConnection;

        public BettingSQLiteRepository(SqliteConnection sqliteConnection, bool refresh)
        {
            _sqliteConnection = sqliteConnection;

            if (refresh)
            {
                DropTables();
            }

            Setup();
        }

        public void DropTables()
        {
            _sqliteConnection.Execute("DROP TABLE IF EXISTS Player");
            _sqliteConnection.Execute("DROP TABLE IF EXISTS BetHistory");
            _sqliteConnection.Execute("DROP TABLE IF EXISTS Better");
            _sqliteConnection.Execute("DROP TABLE IF EXISTS Match");
            _sqliteConnection.Execute("DROP TABLE IF EXISTS Tournament");
        }

        #region Setup
        
        private void Setup()
        {
            const string createTournamentTableSql = @"CREATE TABLE IF NOT EXISTS Tournament
                                                    (
                                                        TournamentId INTEGER PRIMARY KEY,
                                                        Name TEXT NOT NULL,
                                                        UNIQUE(Name)
                                                    )";
            _sqliteConnection.Execute(createTournamentTableSql);

            const string createMatchTableSql = @"CREATE TABLE IF NOT EXISTS Match 
                                                (
                                                    MatchId INTEGER PRIMARY KEY,
                                                    TournamentId INTEGER NOT NULL,
                                                    Name TEXT NOT NULL,
                                                    Status TEXT NOT NULL,
                                                    WinningTeamNumber INTEGER,
                                                    UNIQUE(TournamentId, Name),
                                                    FOREIGN KEY(TournamentId) REFERENCES Tournament(TournamentId)
                                                )";
            _sqliteConnection.Execute(createMatchTableSql);

            const string createBetterTableSql = @"CREATE TABLE IF NOT EXISTS Better
                                                (
                                                    BetterId INTEGER PRIMARY KEY,
                                                    Name TEXT NOT NULL,
                                                    TournamentId INTEGER NOT NULL,
                                                    Balance REAL NOT NULL,
                                                    FOREIGN KEY(TournamentId) REFERENCES Tournament(TournamentId),
                                                    UNIQUE(TournamentId, Name)
                                                )";
            _sqliteConnection.Execute(createBetterTableSql);

            const string createBetHistoryTableSql = @"CREATE TABLE IF NOT EXISTS BetHistory
                                                    (
                                                        BetId INTEGER PRIMARY KEY,
                                                        BetterId INTEGER NOT NULL,
                                                        MatchId INTEGER NOT NULL,
                                                        Amount REAL NOT NULL,
                                                        TeamNumber INTEGER NOT NULL,
                                                        Won INTEGER NOT NULL,
                                                        FOREIGN KEY(BetterId) REFERENCES Better(BetterId),
                                                        FOREIGN KEY(MatchId) REFERENCES Match(MatchId)
                                                    )";
            _sqliteConnection.Execute(createBetHistoryTableSql);

            const string createPlayerTableSql = @"CREATE TABLE IF NOT EXISTS Player
                                                    (
                                                        PlayerId INTEGER PRIMARY KEY,
                                                        Name TEXT NOT NULL,
                                                        MatchId INTEGER NOT NULL,
                                                        TeamNumber INTEGER NOT NULL,
                                                        FOREIGN KEY(MatchId) REFERENCES Match(MatchId)
                                                    )";
            _sqliteConnection.Execute(createPlayerTableSql);
        }

        #endregion
        #region Methods

        public bool DoesTournamentExist(string tournamentName)
        {
            const string sql = "SELECT COUNT(1) FROM Tournament WHERE Name=@tournamentName";

            return _sqliteConnection.ExecuteScalar<bool>(sql, new { tournamentName });
        }

        public bool DoesMatchExist(string tournamentName, string matchName)
        {
            return GetMatch(tournamentName, matchName) != null;
        }

        public bool DoesBetterExist(string tournamentName, string betterName)
        {
            return GetBetterInfo(tournamentName, betterName) != null;
        }

        public void StartNewTournament(string tournamentName)
        {
            const string sql = "INSERT INTO Tournament (Name) VALUES (@tournamentName)";

            _sqliteConnection.Execute(sql, new { tournamentName });
        }

        public void AddMatch(string tournamentName, string matchName, IEnumerable<string> team1, IEnumerable<string> team2)
        {
            const string sqlMatch = "INSERT INTO Match (TournamentId, Name, Status) VALUES (@tournamentId, @matchName, @status)";

            _sqliteConnection.Open();
            var transaction = _sqliteConnection.BeginTransaction();

            try
            {
                var tournamentId = GetTournamentId(tournamentName);

                _sqliteConnection.Execute(sqlMatch,
                    new {tournamentId, matchName, status = MatchStatus.WaitingToStart.ToString()});

                var matchId = GetMatchId(tournamentId, matchName);

                AddPlayers(matchId, team1, team2);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void StartMatch(string tournamentName, string matchName)
        {
            _sqliteConnection.Open();
            var transaction = _sqliteConnection.BeginTransaction();

            try
            {
                var (_, matchId) = GetTournamentAndMatchId(tournamentName, matchName);

                UpdateMatchStatus(matchId, MatchStatus.Running);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void RemoveMatch(string tournamentName, string matchName)
        {
            _sqliteConnection.Open();
            var transaction = _sqliteConnection.BeginTransaction();

            try
            {
                var (tournamentId, matchId) = GetTournamentAndMatchId(tournamentName, matchName);

                var bets = GetBets(matchId).ToList();

                if (bets.Any())
                {
                    ReverseBets(matchId, bets);
                }

                RemovePlayers(matchId);
                RemoveMatch(matchId);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void DeclareMatchWinner(string tournamentName, string matchName, int teamNumber)
        {
            _sqliteConnection.Open();
            var transaction = _sqliteConnection.BeginTransaction();

            try
            {
                var (_, matchId) = GetTournamentAndMatchId(tournamentName, matchName);

                UpdateMatchStatus(matchId, MatchStatus.Finished);
                SetMatchWinner(matchId, teamNumber);

                var bets = GetBets(matchId);
                ProcessBets(teamNumber, bets);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public IEnumerable<Match> GetMatches(string tournamentName)
        {
            _sqliteConnection.Open();
            var transaction = _sqliteConnection.BeginTransaction();

            try
            {
                var tournamentId = GetTournamentId(tournamentName);

                var matches = GetMatches(tournamentId).ToList();

                foreach (var match in matches)
                {
                    match.Team1 = GetPlayersOnTeam(match.Id, 1).Select(x => x.Name);
                    match.Team2 = GetPlayersOnTeam(match.Id, 2).Select(x => x.Name);
                }

                transaction.Commit();

                return matches;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void AddBet(string tournamentName, string betterName, string matchName, decimal betAmount, int teamNumber)
        {
            const string sql = @"INSERT INTO BetHistory (BetterId, MatchId, Amount, TeamNumber)
                                    VALUES (@betterId, @matchId, @amount, @teamNumber)";

            _sqliteConnection.Open();
            var transaction = _sqliteConnection.BeginTransaction();

            try
            {
                var better = GetBetterInfo(tournamentName, betterName);

                var (_, matchId) = GetTournamentAndMatchId(tournamentName, matchName);

                _sqliteConnection.Execute(sql,
                    new {betterId = better.Id, matchId, amount = betAmount, teamNumber});

                SubtractFromBetterBalance(better.Id, betAmount);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public MatchResult GetMatchResult(string tournamentName, string matchName)
        {
            _sqliteConnection.Open();
            var transaction = _sqliteConnection.BeginTransaction();

            try
            {
                var match = GetMatch(tournamentName, matchName);

                if (match == null)
                {
                    throw new MatchDoesNotExistsException(matchName);
                }

                var (_, matchId) = GetTournamentAndMatchId(tournamentName, matchName);

                var bets = GetBets(matchId).ToList();

                var matchResult = new MatchResult
                {
                    WinningTeamNumber = match.WinningTeamNumber,
                    WinningBets = bets.Where(x => x.Won == true),
                    LosingBets = bets.Where(x => x.Won == false)
                };

                transaction.Commit();

                return matchResult;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public decimal GetBalance(string tournamentName, string betterName)
        {
            return GetBetterInfo(tournamentName, betterName).Balance;
        }

        public IEnumerable<Better> GetLeaderBoard(string tournamentName)
        {
            const string sql = @"SELECT BetterId as Id, Name, TournamentId, Balance FROM Better WHERE TournamentId=@tournamentId";

            _sqliteConnection.Open();
            var transaction = _sqliteConnection.BeginTransaction();

            try
            {
                var tournamentId = GetTournamentId(tournamentName);

                var betters = _sqliteConnection.Query<Better>(sql, new {tournamentId}).ToList();

                foreach (var better in betters)
                {
                    better.Bets = GetPlayerBets(tournamentId, better.Name);
                }

                transaction.Commit();

                return betters;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public Better GetBetterInfo(string tournamentName, string betterName)
        {
            const string sql = "SELECT BetterId AS Id, Name, TournamentId, Balance FROM Better WHERE TournamentId=@tournamentId AND BetterName=@betterName";

            _sqliteConnection.Open();
            var transaction = _sqliteConnection.BeginTransaction();

            try
            {
                var tournamentId = GetTournamentId(tournamentName);

                var better = _sqliteConnection.QueryFirstOrDefault<Better>(sql, new {tournamentId, betterName});

                better.Bets = GetPlayerBets(tournamentId, better.Name);

                transaction.Commit();

                return better;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public Match GetMatch(string tournamentName, string matchName)
        {
            const string sql = "SELECT MatchId AS Id, TournamentId, Name, Status, WinningTeamNumber FROM Match WHERE MatchId=@matchId";

            _sqliteConnection.Open();
            var transaction = _sqliteConnection.BeginTransaction();

            try
            {
                var (_, matchId) = GetTournamentAndMatchId(tournamentName, matchName);

                var match = _sqliteConnection.QueryFirstOrDefault<Match>(sql, new {matchId});

                if (match == null)
                {
                    transaction.Commit();
                    return null;
                }

                match.Team1 = GetPlayersOnTeam(matchId, 1).Select(x => x.Name);
                match.Team2 = GetPlayersOnTeam(matchId, 2).Select(x => x.Name);

                transaction.Commit();

                return match;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        #endregion
        #region Helper

        private IEnumerable<Player> GetPlayersOnTeam(long matchId, int teamNumber)
        {
            const string sql =
                "SELECT PlayerId AS Id, Name, MatchId, TeamNumber FROM Player WHERE MatchId=@matchId and TeamNumber=@teamNumber";

            return _sqliteConnection.Query<Player>(sql, new {matchId, teamNumber});
        }

        private long GetTournamentId(string tournamentName)
        {
            const string sql = "SELECT TournamentId FROM Tournament WHERE Name=@tournamentName";

            return _sqliteConnection.ExecuteScalar<int>(sql, new { tournamentName });
        }

        private long GetMatchId(long tournamentId, string matchName)
        {
            const string sql = "SELECT MatchId FROM Match WHERE TournamentId=@tournamentId AND Name=@matchName";

            return _sqliteConnection.ExecuteScalar<int>(sql, new { tournamentId, matchName });
        }

        private (long TournamentId, long MatchId) GetTournamentAndMatchId(string tournamentName, string matchName)
        {
            var tournamentId = GetTournamentId(tournamentName);
            var matchId = GetMatchId(tournamentId, matchName);

            return (tournamentId, matchId);
        }

        private void AddPlayers(long matchId, IEnumerable<string> team1, IEnumerable<string> team2)
        {
            const string sqlPlayer = "INSERT INTO Player (Name, MatchId, TeamNumber) VALUES (@name, @matchId, @teamNumber)";

            foreach (var player in team1)
            {
                _sqliteConnection.Execute(sqlPlayer, new { name = player, matchId, teamNumber = 1 });
            }

            foreach (var player in team2)
            {
                _sqliteConnection.Execute(sqlPlayer, new { name = player, matchId, teamNumber = 2 });
            }
        }

        private IEnumerable<Bet> GetBets(long matchId)
        {
            const string sql = GetBetsSql + "BetHistory.MatchId=@matchId";

            return _sqliteConnection.Query<Bet>(sql, new {matchId});
        }

        private IEnumerable<Bet> GetPlayerBets(long tournamentId, string betterName)
        {
            const string sql = GetBetsSql + "BetHistory.TournamentId=@tournamentId AND BetHistory.Name=@betterName";

            return _sqliteConnection.Query<Bet>(sql, new {tournamentId, betterName});
        }

        private void RemovePlayers(long matchId)
        {
            const string sql = "DELETE FROM Player WHERE Match=@matchId";

            _sqliteConnection.Execute(sql, new {matchId});
        }

        private void RemoveMatch(long matchId)
        {
            const string sql = "DELETE FROM Match WHERE Match=@matchId";

            _sqliteConnection.Execute(sql, new {matchId});
        }

        private void ReverseBets(long matchId, IEnumerable<Bet> bets)
        {
            const string sql = @"UPDATE Better SET Balance = Balance + @amount WHERE BetterId=@betterId";

            foreach (var bet in bets)
            {
                _sqliteConnection.Execute(sql, new { bet.Amount, bet.BetterId });
            }

            DeleteBets(matchId);
        }

        private void DeleteBets(long matchId)
        {
            const string sql = @"DELETE FROM BetHistory WHERE MatchId=@matchId";

            _sqliteConnection.Execute(sql, new {matchId});
        }

        private void ProcessBets(int winningTeamNumber, IEnumerable<Bet> bets)
        {
            const string betterBalanceSql = @"UPDATE Better SET Balance = Balance + (@amount * 2) WHERE BetterId=@betterId AND 1=@won";
            const string updateBetSql = @"UPDATE BetHistory SET Won=@won WHERE BetId=@betId";

            foreach (var bet in bets)
            {
                _sqliteConnection.Execute(betterBalanceSql, new { won = bet.TeamNumber == winningTeamNumber ? 1 : 0, bet.Amount, bet.BetterId });
                _sqliteConnection.Execute(updateBetSql, new {won = bet.TeamNumber == winningTeamNumber ? 1 : 0, betId = bet.Id});
            }
        }

        private void UpdateMatchStatus(long matchId, MatchStatus status)
        {
            const string sql = "UPDATE Match SET Status=@status WHERE Match=@matchId";

            _sqliteConnection.Execute(sql, new { status = status.ToString(), matchId });
        }

        private void SetMatchWinner(long matchId, int winningTeamNumber)
        {
            const string sql = "UPDATE Match SET WinningTeamNumber=@winningTeamNumber WHERE Match=@matchId";

            _sqliteConnection.Execute(sql, new { winningTeamNumber, matchId });
        }

        private IEnumerable<Match> GetMatches(long tournamentId)
        {
            const string sql = @"SELECT MatchId AS Id, TournamentId, Name, WinningTeamNumber, Status FROM Match WHERE TournamentId=@tournamentId";

            return _sqliteConnection.Query<Match>(sql, new {tournamentId});
        }

        private void SubtractFromBetterBalance(long betterId, decimal amount)
        {
            const string sql = @"UPDATE Better SET Balance = Balance - @amount WHERE BetterId=@betterId";

            _sqliteConnection.Execute(sql, new {betterId, amount});
        }

        #endregion
        #region Consts

        private const string GetBetsSql = @"SELECT
                                                BetHistory.BetId AS Id,
                                                BetHistory.BetterId AS BetterId,
                                                Better.Name AS BetterName,
                                                Match.Name AS MatchName,
                                                Amount,
                                                Won,
                                                TeamNumber
                                            FROM
                                                BetHistory
                                            JOIN Better ON BetHistory.BetterId = Better.BetterId
                                            JOIN Match ON BetHistory.MatchId = Match.MatchId
                                            WHERE ";

        #endregion
    }
}
