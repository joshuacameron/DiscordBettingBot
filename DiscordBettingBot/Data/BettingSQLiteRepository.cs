using Dapper;
using DiscordBettingBot.Data.Interfaces;
using DiscordBettingBot.Data.Models;
using DiscordBettingBot.Service.Enumerations;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBettingBot.Data
{
    public class BettingSQLiteRepository : IBettingRepository
    {
        private readonly SqliteConnection _sqliteConnection;
        private SqliteTransaction _sqliteTransaction;

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
                                                        Id INTEGER PRIMARY KEY,
                                                        Name TEXT NOT NULL,
                                                        UNIQUE(Name)
                                                    )";
            _sqliteConnection.Execute(createTournamentTableSql);

            const string createMatchTableSql = @"CREATE TABLE IF NOT EXISTS Match 
                                                (
                                                    Id INTEGER PRIMARY KEY,
                                                    TournamentId INTEGER NOT NULL,
                                                    Name TEXT NOT NULL,
                                                    Status TEXT NOT NULL,
                                                    WinningTeamNumber INTEGER,
                                                    UNIQUE(TournamentId, Name),
                                                    FOREIGN KEY(TournamentId) REFERENCES Tournament(Id)
                                                )";
            _sqliteConnection.Execute(createMatchTableSql);

            const string createBetterTableSql = @"CREATE TABLE IF NOT EXISTS Better
                                                (
                                                    Id INTEGER PRIMARY KEY,
                                                    Name TEXT NOT NULL,
                                                    TournamentId INTEGER NOT NULL,
                                                    Balance REAL NOT NULL,
                                                    FOREIGN KEY(TournamentId) REFERENCES Tournament(Id),
                                                    UNIQUE(TournamentId, Name)
                                                )";
            _sqliteConnection.Execute(createBetterTableSql);

            const string createBetHistoryTableSql = @"CREATE TABLE IF NOT EXISTS BetHistory
                                                    (
                                                        Id INTEGER PRIMARY KEY,
                                                        BetterId INTEGER NOT NULL,
                                                        MatchId INTEGER NOT NULL,
                                                        Amount REAL NOT NULL,
                                                        TeamNumber INTEGER NOT NULL,
                                                        Won INTEGER,
                                                        FOREIGN KEY(BetterId) REFERENCES Better(Id),
                                                        FOREIGN KEY(MatchId) REFERENCES Match(Id)
                                                    )";
            _sqliteConnection.Execute(createBetHistoryTableSql);

            const string createPlayerTableSql = @"CREATE TABLE IF NOT EXISTS Player
                                                    (
                                                        Id INTEGER PRIMARY KEY,
                                                        Name TEXT NOT NULL,
                                                        MatchId INTEGER NOT NULL,
                                                        TeamNumber INTEGER NOT NULL,
                                                        FOREIGN KEY(MatchId) REFERENCES Match(Id)
                                                    )";
            _sqliteConnection.Execute(createPlayerTableSql);
        }

        #endregion
        #region Methods

        public void InsertTournament(string tournamentName)
        {
            const string sql = @"INSERT INTO Tournament (Name) VALUES (@tournamentName)";

            _sqliteConnection.Execute(sql, new {tournamentName});
        }

        public long InsertMatch(Match match)
        {
            const string sql = @"INSERT INTO Match (TournamentId, Name, Status) VALUES (@TournamentId, @Name, @Status);SELECT last_insert_rowid();";

            return _sqliteConnection.ExecuteScalar<long>(sql, match);
        }

        public Match GetMatchByName(long tournamentId, string matchName)
        {
            const string sql = @"SELECT * FROM Match WHERE TournamentId=@tournamentId AND Name=@matchName";

            var match = _sqliteConnection.QueryFirstOrDefault<Match>(sql, new {tournamentId, matchName});

            if (match == null)
            {
                return null;
            }

            var players = GetPlayerByMatchId(match.Id);

            match.Team1 = players.Where(x => x.TeamNumber == 1).ToList();
            match.Team2 = players.Where(x => x.TeamNumber == 2).ToList();

            return match;
        }

        public List<Player> GetPlayerByMatchId(long matchId)
        {
            const string sql = @"SELECT * FROM Player WHERE MatchId=@matchId";

            return _sqliteConnection.Query<Player>(sql, new {matchId}).ToList();
        }

        public void UpdateMatchStatus(long matchId, MatchStatus matchStatus)
        {
            const string sql = @"UPDATE Match SET Status=@matchStatus WHERE Id=@matchId";

            _sqliteConnection.Execute(sql, new {matchId, matchStatus});
        }

        public void UpdateMatchWinningTeamNumber(long matchId, int winningTeamNumber)
        {
            const string sql = @"UPDATE Match SET WinningTeamNumber=@winningTeamNumber WHERE Id=@matchId";

            _sqliteConnection.Execute(sql, new {matchId, winningTeamNumber});
        }

        public void DeleteMatch(long matchId)
        {
            const string sql = @"DELETE FROM Match WHERE Id=@matchId";

            _sqliteConnection.Execute(sql, new {matchId});
        }

        public List<Match> GetMatchesByTournamentId(long tournamentId)
        {
            const string sql = @"SELECT * FROM Match WHERE TournamentId=@tournamentId";

            var matches = _sqliteConnection.Query<Match>(sql, new {tournamentId}).ToList();

            foreach (var match in matches)
            {
                var players = GetPlayerByMatchId(match.Id);

                match.Team1 = players.Where(x => x.TeamNumber == 1).ToList();
                match.Team2 = players.Where(x => x.TeamNumber == 2).ToList();
            }

            return matches;
        }

        public void InsertPlayers(List<Player> players)
        {
            const string sql = @"INSERT INTO Player (Name, MatchId, TeamNumber) VALUES (@Name, @MatchId, @TeamNumber)";

            _sqliteConnection.Execute(sql, players);
        }

        public void DeletePlayerByPlayerIds(List<long> playerIds)
        {
            const string sql = @"DELETE FROM Player WHERE Id IN @playerIds";

            _sqliteConnection.Execute(sql, new {playerIds});
        }

        public List<Bet> GetBetsByMatchId(long matchId)
        {
            const string sql = @"SELECT * FROM BetHistory WHERE MatchId=matchId";

            return _sqliteConnection.Query<Bet>(sql, new {matchId}).ToList();
        }

        public List<Bet> GetBetsByBetterId(long betterId)
        {
            const string sql = @"SELECT * FROM BetHistory WHERE BetterId=@betterId";

            return _sqliteConnection.Query<Bet>(sql, new {betterId}).ToList();
        }

        public void DeleteBetsById(List<long> betIds)
        {
            const string sql = @"DELETE FROM BetHistory WHERE Id IN @betIds";

            _sqliteConnection.Execute(sql, new { betIds });
        }

        public void AddToBetterAmounts(List<long> betterIds, List<decimal> amounts)
        {
            if (betterIds.Count != amounts.Count)
            {
                throw new ArgumentException("Arrays must be same length");
            }

            const string sql = @"UPDATE Better SET Balance = Balance + @amount WHERE Id=@betterId";

            for (int i = 0; i < betterIds.Count; i++)
            {
                _sqliteConnection.Execute(sql, new {betterId = betterIds[i], amount = amounts[i]});
            }
        }

        public void UpdateBets(List<Bet> bets)
        {
            const string sql = @"UPDATE BetHistory SET
                                    BetterId=@BetterId,
                                    MatchId=@MatchId,
                                    Amount=@Amount,
                                    TeamNumber=@TeamNumber,
                                    Won=@Won
                                WHERE Id=@Id";

            _sqliteConnection.Execute(sql, bets);
        }

        public void AddBet(Bet bet)
        {
            const string sql = @"INSERT INTO BetHistory (BetterId, MatchId, Amount, TeamNumber) VALUES (@BetterId, @MatchId, @Amount, @TeamNumber)";

            _sqliteConnection.Execute(sql, bet);
        }

        public MatchResult GetMatchResult(long matchId)
        {
            var bets = GetBetsByMatchId(matchId);
            var match = GetMatchByMatchId(matchId);

            return new MatchResult()
            {
                WinningTeamNumber = match.WinningTeamNumber,
                WinningBets = bets.Where(x => x.Won == true),
                LosingBets = bets.Where(x => x.Won == false)
            };
        }

        public void InsertBetter(long tournamentId, string betterName, decimal initialBalance)
        {
            const string sql = @"INSERT INTO Better (Name, TournamentId, Balance) VALUES (@betterName, @tournamentId, @initialBalance)";

            _sqliteConnection.Execute(sql, new { betterName, tournamentId, initialBalance});
        }

        private Match GetMatchByMatchId(long matchId)
        {
            const string sql = @"SELECT * FROM Match WHERE Id=@matchId";

            var match = _sqliteConnection.QueryFirstOrDefault<Match>(sql, new {matchId});

            var players = GetPlayerByMatchId(match.Id);

            match.Team1 = players.Where(x => x.TeamNumber == 1).ToList();
            match.Team2 = players.Where(x => x.TeamNumber == 2).ToList();

            return match;
        }

        public Better GetBetterByName(long tournamentId, string betterName)
        {
            const string sql = @"SELECT * FROM Better WHERE TournamentId=@tournamentId AND Name=@betterName";

            var better = _sqliteConnection.QueryFirstOrDefault<Better>(sql, new {tournamentId, betterName});

            if (better == null)
            {
                return null;
            }

            better.Bets = GetBetsByBetterId(better.Id);

            return better;
        }

        private Better GetBetterById(long betterId)
        {
            const string sql = @"SELECT * FROM Better WHERE Id=@betterId";

            var better = _sqliteConnection.QueryFirstOrDefault<Better>(sql, new {betterId});

            if (better == null)
            {
                return null;
            }

            better.Bets = GetBetsByBetterId(betterId);

            return better;
        }

        public List<Better> GetBetterByTournamentId(long tournamentId)
        {
            const string sql = @"SELECT * FROM Better WHERE TournamentId=@tournamentId";

            var betters = _sqliteConnection.Query<Better>(sql, new {tournamentId});

            return betters.Select(x =>
            {
                x.Bets = GetBetsByBetterId(x.Id);
                return x;
            }).ToList();
        }

        public Tournament GetTournamentByName(string tournamentName)
        {
            const string sql = @"SELECT * FROM Tournament WHERE Name=@tournamentName";

            return _sqliteConnection.QueryFirstOrDefault<Tournament>(sql, new {tournamentName});
        }

        #endregion
        #region Transaction

        public void BeginTransaction()
        {
            _sqliteConnection.Open();
            _sqliteTransaction = _sqliteConnection.BeginTransaction();
        }
        public void RollbackTransaction() => _sqliteTransaction.Rollback();
        public void CommitTransaction() => _sqliteTransaction.Commit();
        
        #endregion
    }
}
