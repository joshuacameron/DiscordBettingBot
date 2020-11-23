using DiscordBettingBot.Data.Interfaces;
using DiscordBettingBot.Service;
using DiscordBettingBot.Service.Interfaces;
using Moq;
using NUnit.Framework;

namespace DiscordBettingBotUnitTests.Service
{
    public class BettingServiceTests
    {
        private Mock<IBettingRepository> _bettingRepository;

        private const string TournamentName = "TournamentName";

        private IBettingService GetService()
        {
            _bettingRepository = new Mock<IBettingRepository>();

            return new BettingService(_bettingRepository.Object);
        }

        [Test]
        public void StartNewTournament_When_TournamentNotExist_Should_StartNewTournament()
        {
            var sut = GetService();

            sut.StartNewTournament(TournamentName);

            Verify_DoesTournamentExist(Times.AtLeastOnce());
            Verify_StartNewTournament(Times.Once());
        }

        #region Getters

        #endregion

        #region Setup

        #endregion

        #region Verify

        private void Verify_StartNewTournament(Times times)
        {
            _bettingRepository.Verify(m => m.StartNewTournament(It.IsAny<string>()), times);
        }

        private void Verify_DoesTournamentExist(Times times)
        {
            _bettingRepository.Verify(m => m.DoesTournamentExist(It.IsAny<string>()), times);
        }

        #endregion

        /*
         *        public void StartNewTournament(string tournamentName)
        {
            VerifyTournamentDoesNotExist(tournamentName);

            _bettingRepository.StartNewTournament(tournamentName);
        }


                private void VerifyTournamentDoesNotExist(string tournamentName)
        {
            if (_bettingRepository.DoesTournamentExist(tournamentName))
            {
                throw new TournamentAlreadyExistsException(tournamentName);
            }
        }
         *
         */
    }
}
