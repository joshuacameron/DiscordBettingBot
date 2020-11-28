using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DiscordBettingBot.Data.Interfaces;
using DiscordBettingBot.Data.Models;
using Microsoft.Data.Sqlite;

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
                                                        Won INTEGER NOT NULL,
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

        public Tournament GetTournamentById(long tournamentId)
        {
            const string sql = "SELECT * FROM Tournament WHERE Id=@tournamentId";

            var tournament = _sqliteConnection.QueryFirstOrDefault<Tournament>(sql, new {tournamentId});

            if (tournament == null)
            {
                return null;
            }


        }

        public Tournament GetTournamentByName(string tournamentName)
        {
            const string sql = "SELECT * FROM Tournament WHERE Name=@tournamentName";

            var tournament = _sqliteConnection.QueryFirstOrDefault<Tournament>(sql, new {tournamentName});
        }

        public Match GetMatchById(long matchId)
        {
            const string sql = "SELECT * FROM Match WHERE Id=@matchId";

            var match = _sqliteConnection.QueryFirstOrDefault<Match>(sql, new {matchId});
            var players = GetPlayersByMatchId(matchId).ToList();

            match.Team1 = players.Where(x => x.TeamNumber == 1);
            match.Team2 = players.Where(x => x.TeamNumber == 2);

            return match;
        }

        public List<Match> GetMatchesByTournamentId(long tournamentId)
        {
            const string sql = "SELECT * FROM Match WHERE TournamentId=@tournamentId";

            var matches = _sqliteConnection.Query<Match>(sql, new {tournamentId}).ToList();

            return AddPlayersToMatch(matches);
        }

        public Match GetMatchByName(string matchName)
        {
            const string sql = "SELECT * FROM Match WHERE Name=@matchName";

            var match = _sqliteConnection.QueryFirstOrDefault<Match>(sql, new {matchName});

            return match == null ? null : AddPlayersToMatch(match);
        }

        public IEnumerable<Player> GetPlayersByMatchId(IEnumerable<long> matchIds)
        {
            const string sql = "SELECT * FROM Player WHERE MatchId IN @matchIds";

            return _sqliteConnection.Query<Player>(sql, new {matchIds});
        }

        #region Helper

        private Match AddPlayersToMatch(Match match)
        {
            var players = GetPlayersByMatchId(match.Id).ToList();

            match.Team1 = players.Where(x => x.TeamNumber == 1);
            match.Team2 = players.Where(x => x.TeamNumber == 2);

            return match;
        }

        private List<Match> AddPlayersToMatch(List<Match> matches)
        {
            foreach (var match in matches)
            {
                var players = GetPlayersByMatchId(new []{matches.Select(x => x.Id)}.ToList();

                match.Team1 = players.Where(x => x.TeamNumber == 1);
                match.Team2 = players.Where(x => x.TeamNumber == 2);
            }

            return matches;
        }

        #endregion

        #region Transaction

        public void BeginTransaction()
        {
            _sqliteTransaction = _sqliteConnection.BeginTransaction();
        }
        public void RollbackTransaction() => _sqliteTransaction.Rollback();
        public void CommitTransaction() => _sqliteTransaction.Commit();
        
        #endregion
    }
}
