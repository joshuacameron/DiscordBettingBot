using DiscordBettingBot.Data.Interfaces;
using DiscordBettingBot.Service;
using DiscordBettingBot.Service.Enumerations;
using DiscordBettingBot.Service.Exceptions;
using DiscordBettingBot.Service.Interfaces;
using DiscordBettingBot.Service.Models;
using DiscordBettingBotUnitTests.Attributes;
using Moq;
using NUnit.Framework;
using Match = DiscordBettingBot.Service.Models.Match;

namespace DiscordBettingBotUnitTests.Service
{
    public class BettingServiceTests
    {
        private Mock<IBettingRepository> _bettingRepository;

        private const string TournamentName = "TournamentName";
        private const string InvalidTournamentName = "";
        private const string ValidMatchName = "MatchName";
        private const string InvalidMatchName = "";
        private const string ValidPlayerName = "PlayerName";
        private const string InvalidPlayerName = "";

        private IBettingService GetService()
        {
            _bettingRepository = new Mock<IBettingRepository>();

            return new BettingService(_bettingRepository.Object);
        }

        [Test]
        public void StartNewTournament_When_TournamentNotExist_Should_StartNewTournament()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsFalse();

            sut.StartNewTournament(TournamentName);

            Verify_DoesTournamentExist(Times.Once());
            Verify_StartNewTournament(Times.Once());
        }

        [Test]
        [ExpectedException(typeof(TournamentAlreadyExistsException))]
        public void StartNewTournament_When_TournamentExists_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();

            sut.StartNewTournament(TournamentName);

            Verify_DoesTournamentExist(Times.Once());
            Verify_StartNewTournament(Times.Never());
        }

        [Test]
        [ExpectedException(typeof(InvalidTournamentNameException))]
        public void StartNewTournament_When_InvalidTournamentName_Should_ThrowException()
        {
            var sut = GetService();

            sut.StartNewTournament(InvalidTournamentName);

            Verify_DoesTournamentExist(Times.Once());
            Verify_StartNewTournament(Times.Once());
        }

        [Test]
        public void AddMatch_When_HappyPath_Should_AddMatch()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsFalse();

            sut.AddMatch(TournamentName, ValidMatchName, new[] {ValidPlayerName}, new[] {ValidPlayerName});

            Verify_DoesTournamentExist(Times.Once());
            Verify_DoesMatchExist(Times.Once());
            Verify_AddMatch(Times.Once());
        }

        [Test]
        [ExpectedException(typeof(InvalidTournamentNameException))]
        public void AddMatch_When_InvalidTournamentName_Should_ThrowException()
        {
            var sut = GetService();

            sut.AddMatch(InvalidTournamentName, ValidMatchName, new[] { ValidPlayerName }, new[] { ValidPlayerName });

            Verify_DoesTournamentExist(Times.Never());
            Verify_DoesMatchExist(Times.Never());
            Verify_AddMatch(Times.Never());
        }

        [Test]
        [ExpectedException(typeof(InvalidMatchNameException))]
        public void AddMatch_When_InvalidMatchName_Should_ThrowException()
        {
            var sut = GetService();

            sut.AddMatch(TournamentName, InvalidMatchName, new[] { ValidPlayerName }, new[] { ValidPlayerName });

            Verify_DoesTournamentExist(Times.Never());
            Verify_DoesMatchExist(Times.Never());
            Verify_AddMatch(Times.Never());
        }

        [Test]
        [ExpectedException(typeof(InvalidPlayerNameException))]
        public void AddMatch_When_InvalidPlayerNameTeam1_Should_ThrowException()
        {
            var sut = GetService();

            sut.AddMatch(TournamentName, ValidMatchName, new[] { InvalidPlayerName }, new[] { ValidPlayerName });

            Verify_DoesTournamentExist(Times.Never());
            Verify_DoesMatchExist(Times.Never());
            Verify_AddMatch(Times.Never());
        }

        [Test]
        [ExpectedException(typeof(InvalidPlayerNameException))]
        public void AddMatch_When_InvalidPlayerNameTeam2_Should_ThrowException()
        {
            var sut = GetService();

            sut.AddMatch(TournamentName, ValidMatchName, new[] { ValidPlayerName }, new[] { InvalidPlayerName });

            Verify_DoesTournamentExist(Times.Never());
            Verify_DoesMatchExist(Times.Never());
            Verify_AddMatch(Times.Never());
        }

        [Test]
        [ExpectedException(typeof(TournamentDoesNotExistException))]
        public void AddMatch_When_TournamentDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsFalse();
            SetupBettingRepository_DoesMatchExist_ReturnsTrue();
            
            sut.AddMatch(TournamentName, ValidMatchName, new[] { ValidPlayerName }, new[] { ValidPlayerName });

            Verify_DoesTournamentExist(Times.Once());
            Verify_AddMatch(Times.Never());
        }

        [Test]
        [ExpectedException(typeof(MatchAlreadyExistsException))]
        public void AddMatch_When_MatchAlreadyExists_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsTrue();

            sut.AddMatch(TournamentName, ValidMatchName, new[] { ValidPlayerName }, new[] { ValidPlayerName });

            Verify_DoesMatchExist(Times.Once());
            Verify_AddMatch(Times.Never());
        }

        [Test]
        public void StartMatch_When_HappyPath_Should_StartMatch()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsTrue();
            SetupBettingRepository_GetMatch_ReturnsWaitingToStart();

            sut.StartMatch(TournamentName, ValidMatchName);

            Verify_DoesTournamentExist(Times.Once());
            Verify_DoesMatchExist(Times.Once());
            Verify_GetMatch(Times.Once());
            Verify_StartMatch(Times.Once());
        }

        [Test]
        [ExpectedException(typeof(InvalidTournamentNameException))]
        public void StartMatch_When_InvalidTournamentName_Should_ThrowException()
        {
            var sut = GetService();

            sut.StartMatch(InvalidTournamentName, ValidMatchName);

            Verify_DoesTournamentExist(Times.Once());
            Verify_StartMatch(Times.Never());
        }

        [Test]
        [ExpectedException(typeof(InvalidMatchNameException))]
        public void StartMatch_When_InvalidMatchName_Should_ThrowException()
        {
            var sut = GetService();

            sut.StartMatch(TournamentName, InvalidMatchName);

            Verify_DoesMatchExist(Times.Once());
            Verify_StartMatch(Times.Never());
        }

        [Test]
        [ExpectedException(typeof(TournamentDoesNotExistException))]
        public void StartMatch_When_TournamentDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsFalse();

            sut.StartMatch(TournamentName, ValidMatchName);

            Verify_DoesTournamentExist(Times.Once());
            Verify_StartMatch(Times.Never());
        }

        [Test]
        [ExpectedException(typeof(MatchDoesNotExistsException))]
        public void StartMatch_When_MatchDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsFalse();

            sut.StartMatch(TournamentName, ValidMatchName);

            Verify_DoesTournamentExist(Times.Once());
            Verify_DoesMatchExist(Times.Once());
            Verify_StartMatch(Times.Never());
        }

        [Test]
        [ExpectedException(typeof(MatchNotWaitingToStartException))]
        public void StartMatch_When_MatchFinished_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsTrue();
            SetupBettingRepository_GetMatch_ReturnsFinished();

            sut.StartMatch(TournamentName, ValidMatchName);

            Verify_GetMatch(Times.Once());
            Verify_StartMatch(Times.Never());
        }

        [Test]
        [ExpectedException(typeof(MatchNotWaitingToStartException))]
        public void StartMatch_When_MatchRunning_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsTrue();
            SetupBettingRepository_GetMatch_ReturnsRunning();

            sut.StartMatch(TournamentName, ValidMatchName);

            Verify_GetMatch(Times.Once());
            Verify_StartMatch(Times.Never());
        }

        /* public void RemoveMatch_When_HappyPath_Should_StartMatch()
         * public void RemoveMatch_When_InvalidTournamentName_Should_ThrowException()
         * public void RemoveMatch_When_InvalidMatchName_Should_ThrowException()
         * public void RemoveMatch_When_TournamentDoesNotExist_Should_ThrowException()
         * public void RemoveMatch_When_MatchDoesNotExist_Should_ThrowException()
         */




        #region Getters

        #endregion

        #region Setup

        private void SetupBettingRepository_DoesTournamentExist_ReturnsTrue()
        {
            _bettingRepository.Setup(m => m.DoesTournamentExist(It.IsAny<string>()))
                .Returns(true);
        }

        private void SetupBettingRepository_DoesTournamentExist_ReturnsFalse()
        {
            _bettingRepository.Setup(m => m.DoesTournamentExist(It.IsAny<string>()))
                .Returns(false);
        }

        private void SetupBettingRepository_DoesMatchExist_ReturnsTrue()
        {
            _bettingRepository.Setup(m => m.DoesMatchExist(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
        }

        private void SetupBettingRepository_DoesMatchExist_ReturnsFalse()
        {
            _bettingRepository.Setup(m => m.DoesMatchExist(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);
        }

        private void SetupBettingRepository_GetMatch_ThrowsNotExist()
        {
            _bettingRepository.Setup(m => m.GetMatch(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<MatchDoesNotExistsException>();
        }

        private void SetupBettingRepository_GetMatch_ReturnsWaitingToStart()
        {
            _bettingRepository.Setup(m => m.GetMatch(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Match() {Status = MatchStatus.WaitingToStart});
        }

        private void SetupBettingRepository_GetMatch_ReturnsRunning()
        {
            _bettingRepository.Setup(m => m.GetMatch(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Match() { Status = MatchStatus.Running });
        }

        private void SetupBettingRepository_GetMatch_ReturnsFinished()
        {
            _bettingRepository.Setup(m => m.GetMatch(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Match() { Status = MatchStatus.Finished });
        }

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

        private void Verify_DoesMatchExist(Times times)
        {
            _bettingRepository.Verify(m => m.DoesMatchExist(It.IsAny<string>(), It.IsAny<string>()), times);
        }

        private void Verify_AddMatch(Times times)
        {
            _bettingRepository.Verify(m => m.AddMatch(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>()), times);
        }

        private void Verify_GetMatch(Times times)
        {
            _bettingRepository.Verify(m => m.GetMatch(It.IsAny<string>(), It.IsAny<string>()), times);
        }

        private void Verify_StartMatch(Times times)
        {
            _bettingRepository.Verify(m => m.StartMatch(It.IsAny<string>(), It.IsAny<string>()), times);
        }

        #endregion
    }
}
