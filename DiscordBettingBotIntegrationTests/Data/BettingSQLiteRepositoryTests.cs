using System.Collections.Generic;
using Autofac;
using DiscordBettingBot.Data.Interfaces;
using NUnit.Framework;

namespace DiscordBettingBotIntegrationTests.Data
{
    public class BettingSQLiteRepositoryTests : IntegrationTestBase
    {
        private IBettingRepository GetService() => Container.Resolve<IBettingRepository>();

        private const string TournamentName = "Tournament1";
        private const string MatchName = "Match1";

        private static IEnumerable<string> Team1Players => new [] { "Player1", "Player2" };
        private static IEnumerable<string> Team2Players => new[] { "Player3", "Player4" };

        [Test]
        public void DoesTournamentExist_WhenTournamentExists_Should_ReturnTrue()
        {
            var sut = GetService();

            var before = sut.DoesTournamentExist(TournamentName);
            sut.StartNewTournament(TournamentName);
            var after = sut.DoesTournamentExist(TournamentName);

            Assert.False(before);
            Assert.True(after);
        }

        [Test]
        public void DoesTournamentExist_WhenTournamentDoesNotExists_Should_ReturnFalse()
        {
            var sut = GetService();

            Assert.False(sut.DoesTournamentExist(TournamentName));
        }

        [Test]
        public void DoesMatchExist_WhenMatchExists_Should_ReturnTrue()
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
        public void DoesMatchExist_WhenMatchDoesNotExists_Should_ReturnFalse()
        {
            var sut = GetService();

            Assert.False(sut.DoesMatchExist(TournamentName, MatchName));
        }
    }
}
