using Dapper;
using DiscordBettingBot.Data.Interfaces;
using DiscordBettingBot.Service.Models;
using Microsoft.Data.Sqlite;
using System;

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
            _sqliteConnection.Execute("DROP TABLE Tournament");
            _sqliteConnection.Execute("DROP TABLE Match");
            _sqliteConnection.Execute("DROP TABLE Better");
            _sqliteConnection.Execute("DROP TABLE BetHistory");
        }

        private void Setup()
        {
            const string createTournamentTableSql = "CREATE TABLE IF NOT EXISTS Tournament (Name TEXT NOT NULL, UNIQUE(Name))";
            _sqliteConnection.Execute(createTournamentTableSql);

            const string createMatchTableSql = @"CREATE TABLE IF NOT EXISTS Match 
                                                (
                                                    TournamentId INTEGER NOT NULL,
                                                    Name TEXT NOT NULL,
                                                    Status TEXT NOT NULL,
                                                    FOREIGN KEY(TournamentId) REFERENCES Tournament(rowid)
                                                )";
            _sqliteConnection.Execute(createMatchTableSql);

            const string createBetterTableSql = @"CREATE TABLE IF NOT EXISTS Better
                                                (
                                                    BetterName TEXT NOT NULL,
                                                    Balance REAL NOT NULL
                                                )";
            _sqliteConnection.Execute(createBetterTableSql);

            const string createBetHistoryTableSql = @"CREATE TABLE IF NOT EXISTS BetHistory
                                                    (
                                                        BetterId INTEGER NOT NULL,
                                                        TournamentId INTEGER NOT NULL,
                                                        MatchId INTEGER NOT NULL,
                                                        Amount REAL NOT NULL,
                                                        Won INTEGER NOT NULL,
                                                        FOREIGN KEY(BetterId) REFERENCES Better(rowid),
                                                        FOREIGN KEY(TournamentId) REFERENCES Tournament(rowid),
                                                        FOREIGN KEY(MatchId) REFERENCES Match(rowid)
                                                    )";
            _sqliteConnection.Execute(createBetHistoryTableSql);
        }

        public bool DoesTournamentExist(string tournamentName)
        {
            const string sql = "SELECT COUNT(1) FROM Tournament WHERE Name=@tournamentName";

            return _sqliteConnection.ExecuteScalar<bool>(sql, new {tournamentName});
        }

        public void StartNewTournament(string tournamentName)
        {
            const string sql = "INSERT INTO Tournament (Name) VALUES (@tournamentName)";

            _sqliteConnection.Execute(sql, new { tournamentName });
        }

        public bool DoesMatchExist(string tournamentName, string matchName)
        {
            const string sql = "SELECT COUNT(1) FROM Match INNER JOIN Tournament ON Match.TournamentId=Tournament.rowid WHERE Match.Name=@matchName AND Tournament.Name=@tournamentName";

            return _sqliteConnection.ExecuteScalar<bool>(sql, new { matchName, tournamentName });
        }

        public void AddMatch(string tournamentName, string matchName, string[] team1, string[] team2)
        {
            throw new NotImplementedException();
        }

        public bool DoesBetterExist(string tournamentName, string betterName)
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

        public Match GetMatch(string tournamentName, string matchName)
        {
            throw new NotImplementedException();
        }
    }
}
