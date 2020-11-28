using System.Collections.Generic;
using Autofac;
using DiscordBettingBot.Data.Interfaces;
using NUnit.Framework;

namespace DiscordBettingBotIntegrationTests.Data
{
    public class BettingSQLiteRepositoryTests : IntegrationTestBase
    {
        private IBettingRepository3 GetService() => Container.Resolve<IBettingRepository3>();

        private const string TournamentName = "Tournament1";
        private const string MatchName = "Match1";

        private static IEnumerable<string> Team1Players => new [] { "Player1", "Player2" };
        private static IEnumerable<string> Team2Players => new[] { "Player3", "Player4" };

        [Test]
        public void DoesTournamentExist_When_TournamentExists_Should_ReturnTrue()
        {
            var sut = GetService();

            var before = sut.DoesTournamentExist(TournamentName);
            sut.StartNewTournament(TournamentName);
            var after = sut.DoesTournamentExist(TournamentName);

            Assert.False(before);
            Assert.True(after);
        }

        [Test]
        public void DoesTournamentExist_When_TournamentDoesNotExists_Should_ReturnFalse()
        {
            var sut = GetService();

            Assert.False(sut.DoesTournamentExist(TournamentName));
        }

        [Test]
        public void DoesMatchExist_When_MatchExists_Should_ReturnTrue()
        {
            var sut = GetService();

            sut.StartNewTournament(TournamentName);
            var before = sut.DoesMatchExist(TournamentName, MatchName);
            sut.AddMatch(TournamentName, MatchName, Team1Players, Team2Players);
            var after = sut.DoesMatchExist(TournamentName, MatchName);

            Assert.False(before);
            Assert.True(after);
        }

        [Test]
        public void DoesMatchExist_When_MatchDoesNotExists_Should_ReturnFalse()
        {
            var sut = GetService();

            Assert.False(sut.DoesMatchExist(TournamentName, MatchName));
        }

        [Test]
        public void DoesBetterExist_When_BetterExists_Should_ReturnTrue()
        {

        }

        [Test]
        public void DoesBetterExist_When_BetterDoesNotExists_Should_ReturnFalse()
        {

        }

        [Test]
        public void AddMatch_When_MatchDoesNotExist_Should_AddMatch()
        {

        }

        [Test]
        public void AddMatch_When_MatchDoesExist_Should_Throw()
        {

        }

        [Test]
        public void AddMatch_When_WhenTournamentDoesExist_Should_Throw()
        {

        }

        [Test]
        public void AddMatch_When_WhenTeamsAreEmpty_Should_AddMatch()
        {

        }

        [Test]
        public void AddMatch_When_WhenTeamsAreUnbalanced_Should_AddMatch()
        {

        }

        [Test]
        public void StartMatch_When_TournamentDoesNotExist_Should_Throw()
        {

        }

        [Test]
        public void StartMatch_When_MatchDoesNotExist_Should_Throw()
        {

        }

        [Test]
        public void StartMatch_When_TournamentExistsAndMatchExist_Should_StartMatch()
        {

        }

        [Test]
        public void RemoveMatch_When_TournamentDoesNotExist_Should_Throw()
        {

        }

        [Test]
        public void RemoveMatch_When_MatchDoesNotExist_Should_Throw()
        {

        }

        [Test]
        public void RemoveMatch_When_TournamentExistsAndMatchExist_Should_RemoveMatch()
        {

        }

        [Test]
        public void DeclareMatchWinner_When_TournamentDoesNotExist_Should_Throw()
        {

        }

        [Test]
        public void DeclareMatchWinner_When_MatchDoesNotExist_Should_Throw()
        {

        }

        [Test]
        public void DeclareMatchWinner_When_TournamentExistsAndMatchExists_Should_DeclareMatchWinner()
        {

        }

        [Test]
        public void GetMatchResult_When_TournamentDoesNotExist_Should_Throw()
        {

        }

        [Test]
        public void GetMatchResult_When_MatchDoesNotExist_Should_Throw()
        {

        }

        [Test]
        public void GetMatchResult_When_TournamentExistsAndMatchExists_Should_GetMatchResult()
        {

        }

        [Test]
        public void GetBalance_When_TournamentDoesNotExist_Should_Throw()
        {

        }

        [Test]
        public void GetBalance_When_BetterDoesNotExist_Should_Throw()
        {

        }

        [Test]
        public void GetBalance_When_TournamentExistsAndBetterExists_Should_GetBalance()
        {

        }







        [Test]
        public void GetMatches_When_TournamentExistsAndOneMatchExists_Should_GetMatches()
        {

        }

        [Test]
        public void GetMatches_When_TournamentExistsAndThreeMatchesExist_Should_GetMatches()
        {

        }

        [Test]
        public void GetMatches_When_TournamentExistsAndNoMatchExists_Should_GetMatches()
        {

        }

        [Test]
        public void GetMatches_When_TournamentDoesNotExists_Should_Throw()
        {

        }


        /*
            IEnumerable<Match> GetMatches(string tournamentName);
            void AddBet(string tournamentName, string betterName, string matchName, decimal betAmount, int teamNumber);
            IEnumerable<Better> GetLeaderBoard(string tournamentName);
            Better GetBetterInfo(string tournamentName, string betterName);
            void StartNewTournament(string tournamentName);
            Match GetMatch(string tournamentName, string matchName);
         */
    }
}
