using DiscordBettingBot.Data.Interfaces;
using DiscordBettingBot.Service;
using DiscordBettingBot.Service.Enumerations;
using DiscordBettingBot.Service.Exceptions;
using DiscordBettingBot.Service.Interfaces;
using Moq;
using NUnit.Framework;
using Match = DiscordBettingBot.Data.Models.Match;

namespace DiscordBettingBotUnitTests.Service
{
    public class BettingServiceTests
    {
        private Mock<IBettingRepository> _bettingRepository;

        #region Consts
        private const string TournamentName = "TournamentName";
        private const string InvalidTournamentName = "";
        private const string ValidMatchName = "MatchName";
        private const string InvalidMatchName = "";
        private const string ValidPlayerName = "PlayerName";
        private const string InvalidPlayerName = "";
        private const string ValidBetterName = "BetterName";
        private const string InvalidBetterName = "";
        private const int ValidTeamNumber = 1;
        private const int InvalidTeamNumber = -1;
        private const int ValidBetAmount = 1;
        private const int InvalidBetAmount = -1;

        #endregion
        #region Tests
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
        public void StartNewTournament_When_TournamentExists_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();

            Assert.Throws<TournamentAlreadyExistsException>(() => sut.StartNewTournament(TournamentName));

            Verify_DoesTournamentExist(Times.Once());
            Verify_StartNewTournament(Times.Never());
        }

        [Test]
        public void StartNewTournament_When_InvalidTournamentName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidTournamentNameException>(() => sut.StartNewTournament(InvalidTournamentName));

            Verify_StartNewTournament(Times.Never());
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
        public void AddMatch_When_InvalidTournamentName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidTournamentNameException>(() => sut.AddMatch(InvalidTournamentName, ValidMatchName,
                new[] {ValidPlayerName}, new[] {ValidPlayerName}));

            Verify_DoesTournamentExist(Times.Never());
            Verify_AddMatch(Times.Never());
        }

        [Test]
        public void AddMatch_When_InvalidMatchName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidMatchNameException>(() =>
                sut.AddMatch(TournamentName, InvalidMatchName, new[] {ValidPlayerName}, new[] {ValidPlayerName}));

            Verify_DoesMatchExist(Times.Never());
            Verify_AddMatch(Times.Never());
        }

        [Test]
        public void AddMatch_When_InvalidPlayerNameTeam1_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidPlayerNameException>(() =>
                sut.AddMatch(TournamentName, ValidMatchName, new[] { InvalidPlayerName }, new[] { ValidPlayerName }));

            Verify_DoesTournamentExist(Times.Never());
            Verify_DoesMatchExist(Times.Never());
            Verify_AddMatch(Times.Never());
        }

        [Test]
        public void AddMatch_When_InvalidPlayerNameTeam2_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidPlayerNameException>(() =>
                sut.AddMatch(TournamentName, ValidMatchName, new[] { ValidPlayerName }, new[] { InvalidPlayerName }));

            Verify_DoesTournamentExist(Times.Never());
            Verify_DoesMatchExist(Times.Never());
            Verify_AddMatch(Times.Never());
        }

        [Test]
        public void AddMatch_When_TournamentDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsFalse();
            SetupBettingRepository_DoesMatchExist_ReturnsTrue();
            
            Assert.Throws<TournamentDoesNotExistException>(() =>
                sut.AddMatch(TournamentName, ValidMatchName, new[] { ValidPlayerName }, new[] { ValidPlayerName }));

            Verify_DoesTournamentExist(Times.Once());
            Verify_AddMatch(Times.Never());
        }

        [Test]
        public void AddMatch_When_MatchAlreadyExists_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsTrue();

            Assert.Throws<MatchAlreadyExistsException>(() =>
                sut.AddMatch(TournamentName, ValidMatchName, new[] { ValidPlayerName }, new[] { ValidPlayerName }));

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
        public void StartMatch_When_InvalidTournamentName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidTournamentNameException>(() => sut.StartMatch(InvalidTournamentName, ValidMatchName));

            Verify_DoesTournamentExist(Times.Never());
            Verify_StartMatch(Times.Never());
        }

        [Test]
        public void StartMatch_When_InvalidMatchName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidMatchNameException>(() => sut.StartMatch(TournamentName, InvalidMatchName));

            Verify_DoesMatchExist(Times.Never());
            Verify_StartMatch(Times.Never());
        }

        [Test]
        public void StartMatch_When_TournamentDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsFalse();

            Assert.Throws<TournamentDoesNotExistException>(() => sut.StartMatch(TournamentName, ValidMatchName));

            Verify_DoesTournamentExist(Times.Once());
            Verify_StartMatch(Times.Never());
        }

        [Test]
        public void StartMatch_When_MatchDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsFalse();

            Assert.Throws<MatchDoesNotExistsException>(() => sut.StartMatch(TournamentName, ValidMatchName));

            Verify_DoesMatchExist(Times.Once());
            Verify_StartMatch(Times.Never());
        }

        [Test]
        public void StartMatch_When_MatchFinished_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsTrue();
            SetupBettingRepository_GetMatch_ReturnsFinished();

            Assert.Throws<MatchNotWaitingToStartException>(() => sut.StartMatch(TournamentName, ValidMatchName));

            Verify_GetMatch(Times.Once());
            Verify_StartMatch(Times.Never());
        }

        [Test]
        public void StartMatch_When_MatchRunning_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsTrue();
            SetupBettingRepository_GetMatch_ReturnsRunning();

            Assert.Throws<MatchNotWaitingToStartException>(() => sut.StartMatch(TournamentName, ValidMatchName));

            Verify_GetMatch(Times.Once());
            Verify_StartMatch(Times.Never());
        }

        [Test]
        public void RemoveMatch_When_HappyPath_Should_RemoveMatch()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsTrue();

            sut.RemoveMatch(TournamentName, ValidMatchName);

            Verify_DoesTournamentExist(Times.Once());
            Verify_DoesMatchExist(Times.Once());
            Verify_RemoveMatch(Times.Once());
        }
        
        [Test]
        public void RemoveMatch_When_InvalidTournamentName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidTournamentNameException>(() => sut.RemoveMatch(InvalidTournamentName, ValidMatchName));

            Verify_DoesTournamentExist(Times.Never());
            Verify_RemoveMatch(Times.Never());
        }

        [Test]
        public void RemoveMatch_When_InvalidMatchName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidMatchNameException>(() => sut.RemoveMatch(TournamentName, InvalidMatchName));

            Verify_DoesMatchExist(Times.Never());
            Verify_RemoveMatch(Times.Never());
        }

        [Test]
        public void RemoveMatch_When_TournamentDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsFalse();

            Assert.Throws<TournamentDoesNotExistException>(() => sut.RemoveMatch(TournamentName, ValidMatchName));

            Verify_DoesTournamentExist(Times.Once());
            Verify_RemoveMatch(Times.Never());
        }

        [Test]
        public void RemoveMatch_When_MatchDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsFalse();

            Assert.Throws<MatchDoesNotExistsException>(() => sut.RemoveMatch(TournamentName, ValidMatchName));

            Verify_DoesMatchExist(Times.Once());
            Verify_RemoveMatch(Times.Never());
        }

        [Test]
        public void DeclareMatchWinner_When_HappyPath_Should_DeclareMatchWinner()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsTrue();
            SetupBettingRepository_GetMatch_ReturnsRunning();

            sut.DeclareMatchWinner(TournamentName, ValidMatchName, ValidTeamNumber);

            Verify_DoesTournamentExist(Times.Once());
            Verify_DoesMatchExist(Times.Once());
            Verify_DeclareMatchWinner(Times.Once());
            Verify_GetMatchResult(Times.Once());
        }

        [Test]
        public void DeclareMatchWinner_When_InvalidTournamentName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidTournamentNameException>(() =>
                sut.DeclareMatchWinner(InvalidTournamentName, ValidMatchName, ValidTeamNumber));

            Verify_DoesTournamentExist(Times.Never());
            Verify_DeclareMatchWinner(Times.Never());
            Verify_GetMatchResult(Times.Never());
        }

        [Test]
        public void DeclareMatchWinner_When_InvalidMatchName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidMatchNameException>(() =>
                sut.DeclareMatchWinner(TournamentName, InvalidMatchName, ValidTeamNumber));

            Verify_DoesMatchExist(Times.Never());
            Verify_DeclareMatchWinner(Times.Never());
            Verify_GetMatchResult(Times.Never());
        }

        [Test]
        public void DeclareMatchWinner_When_InvalidTeamNumber_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidTeamNumberException>(() =>
                sut.DeclareMatchWinner(TournamentName, ValidMatchName, InvalidTeamNumber));

            Verify_DeclareMatchWinner(Times.Never());
            Verify_GetMatchResult(Times.Never());
        }

        [Test]
        public void DeclareMatchWinner_When_TournamentDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsFalse();

            Assert.Throws<TournamentDoesNotExistException>(() =>
                sut.DeclareMatchWinner(TournamentName, ValidMatchName, ValidTeamNumber));

            Verify_DoesTournamentExist(Times.Once());
            Verify_DeclareMatchWinner(Times.Never());
            Verify_GetMatchResult(Times.Never());
        }

        [Test]
        public void DeclareMatchWinner_When_MatchDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsFalse();

            Assert.Throws<MatchDoesNotExistsException>(() =>
                sut.DeclareMatchWinner(TournamentName, ValidMatchName, ValidTeamNumber));

            Verify_DoesMatchExist(Times.Once());
            Verify_DeclareMatchWinner(Times.Never());
            Verify_GetMatchResult(Times.Never());
        }

        [Test]
        public void GetBalance_When_HappyPath_Should_GetBalance()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesBetterExist_ReturnsTrue();

            sut.GetBalance(TournamentName, ValidBetterName);

            Verify_DoesTournamentExist(Times.Once());
            Verify_DoesBetterExist(Times.Once());
            Verify_GetBalance(Times.Once());
        }

        [Test]
        public void GetBalance_When_InvalidTournamentName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidTournamentNameException>(() => sut.GetBalance(InvalidTournamentName, ValidBetterName));

            Verify_DoesTournamentExist(Times.Never());
            Verify_GetBalance(Times.Never());
        }

        [Test]
        public void GetBalance_When_InvalidBetterName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidBetterNameException>(() => sut.GetBalance(TournamentName, InvalidBetterName));

            Verify_DoesBetterExist(Times.Never());
            Verify_GetBalance(Times.Never());
        }

        [Test]
        public void GetBalance_When_TournamentDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsFalse();
            SetupBettingRepository_DoesBetterExist_ReturnsTrue();

            Assert.Throws<TournamentDoesNotExistException>(() => sut.GetBalance(TournamentName, ValidBetterName));

            Verify_DoesTournamentExist(Times.Once());
            Verify_GetBalance(Times.Never());
        }

        [Test]
        public void GetBalance_When_BetterDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesBetterExist_ReturnsFalse();

            Assert.Throws<BetterDoesNotExistException>(() => sut.GetBalance(TournamentName, ValidBetterName));

            Verify_DoesBetterExist(Times.Once());
            Verify_GetBalance(Times.Never());
        }

        [Test]
        public void GetAvailableMatches_When_HappyPath_Should_GetAvailableMatches()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();

            sut.GetMatches(TournamentName);

            Verify_DoesTournamentExist(Times.Once());
            Verify_GetAvailableMatches(Times.Once());
        }

        [Test]
        public void GetAvailableMatches_When_InvalidTournamentName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidTournamentNameException>(() => sut.GetMatches(InvalidTournamentName));

            Verify_DoesTournamentExist(Times.Never());
            Verify_GetAvailableMatches(Times.Never());
        }

        [Test]
        public void GetAvailableMatches_When_TournamentDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsFalse();

            Assert.Throws<TournamentDoesNotExistException>(() => sut.GetMatches(TournamentName));

            Verify_DoesTournamentExist(Times.Once());
            Verify_GetAvailableMatches(Times.Never());
        }

        [Test]
        public void AddBet_When_HappyPath_Should_AddBet()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsTrue();
            SetupBettingRepository_GetMatch_ReturnsWaitingToStart();

            sut.AddBet(TournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber);

            Verify_DoesTournamentExist(Times.Once());
            Verify_DoesMatchExist(Times.Once());
            Verify_GetMatch(Times.Once());
            Verify_AddBet(Times.Once());
        }

        [Test]
        public void AddBet_When_InvalidTournamentName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidTournamentNameException>(() =>
                sut.AddBet(InvalidTournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber));

            Verify_DoesTournamentExist(Times.Never());
            Verify_GetMatch(Times.Never());
            Verify_AddBet(Times.Never());
        }

        [Test]
        public void AddBet_When_InvalidBetterName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidBetterNameException>(() =>
                sut.AddBet(TournamentName, InvalidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber));

            Verify_GetMatch(Times.Never());
            Verify_AddBet(Times.Never());
        }

        [Test]
        public void AddBet_When_InvalidMatchName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidMatchNameException>(() =>
                sut.AddBet(TournamentName, ValidBetterName, InvalidMatchName, ValidBetAmount, ValidTeamNumber));

            Verify_DoesMatchExist(Times.Never());
            Verify_GetMatch(Times.Never());
            Verify_AddBet(Times.Never());
        }

        [Test]
        public void AddBet_When_InvalidBetAmount_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidBetAmountException>(() =>
                sut.AddBet(TournamentName, ValidBetterName, ValidMatchName, InvalidBetAmount, ValidTeamNumber));

            Verify_GetMatch(Times.Never());
            Verify_AddBet(Times.Never());
        }

        [Test]
        public void AddBet_When_TournamentDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsFalse();

            Assert.Throws<TournamentDoesNotExistException>(() =>
                sut.AddBet(TournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber));

            Verify_DoesTournamentExist(Times.Once());
            Verify_GetMatch(Times.Never());
            Verify_AddBet(Times.Never());
        }

        [Test]
        public void AddBet_When_MatchDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsFalse();

            Assert.Throws<MatchDoesNotExistsException>(() =>
                sut.AddBet(TournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber));

            Verify_DoesMatchExist(Times.Once());
            Verify_GetMatch(Times.Never());
            Verify_AddBet(Times.Never());
        }

        [Test]
        public void AddBet_When_MatchNotWaitingToStart_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesMatchExist_ReturnsTrue();
            SetupBettingRepository_GetMatch_ReturnsRunning();

            Assert.Throws<MatchNotWaitingToStartException>(() =>
                sut.AddBet(TournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber));

            Verify_GetMatch(Times.Once());
            Verify_AddBet(Times.Never());
        }

        [Test]
        public void GetLeaderBoard_When_HappyPath_Should_GetLeaderBoard()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();

            sut.GetLeaderBoard(TournamentName);

            Verify_DoesTournamentExist(Times.Once());
            Verify_GetLeaderBoard(Times.Once());
        }

        [Test]
        public void GetLeaderBoard_When_InvalidTournamentName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidTournamentNameException>(() => sut.GetLeaderBoard(InvalidTournamentName));

            Verify_DoesTournamentExist(Times.Never());
            Verify_GetLeaderBoard(Times.Never());
        }

        [Test]
        public void GetLeaderBoard_When_TournamentDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsFalse();

            Assert.Throws<TournamentDoesNotExistException>(() => sut.GetLeaderBoard(TournamentName));

            Verify_DoesTournamentExist(Times.Once());
            Verify_GetLeaderBoard(Times.Never());
        }

        [Test]
        public void GetBetterInfo_When_HappyPath_Should_GetBetterInfo()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesBetterExist_ReturnsTrue();

            sut.GetBetterInfo(TournamentName, ValidBetterName);

            Verify_DoesTournamentExist(Times.Once());
            Verify_GetBetterInfo(Times.Once());
        }

        [Test]
        public void GetBetterInfo_When_InvalidTournamentName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidTournamentNameException>(() => sut.GetBetterInfo(InvalidTournamentName, ValidBetterName));

            Verify_DoesTournamentExist(Times.Never());
            Verify_GetBetterInfo(Times.Never());
        }

        [Test]
        public void GetBetterInfo_When_InvalidBetterName_Should_ThrowException()
        {
            var sut = GetService();

            Assert.Throws<InvalidBetterNameException>(() => sut.GetBetterInfo(TournamentName, InvalidBetterName));

            Verify_DoesBetterExist(Times.Never());
            Verify_GetBetterInfo(Times.Never());
        }

        [Test]
        public void GetBetterInfo_When_TournamentDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsFalse();

            Assert.Throws<TournamentDoesNotExistException>(() => sut.GetBetterInfo(TournamentName, ValidBetterName));

            Verify_DoesTournamentExist(Times.Once());
            Verify_GetBetterInfo(Times.Never());
        }

        [Test]
        public void GetBetterInfo_When_BetterDoesNotExist_Should_ThrowException()
        {
            var sut = GetService();
            SetupBettingRepository_DoesTournamentExist_ReturnsTrue();
            SetupBettingRepository_DoesBetterExist_ReturnsFalse();

            Assert.Throws<BetterDoesNotExistException>(() => sut.GetBetterInfo(TournamentName, ValidBetterName));

            Verify_DoesBetterExist(Times.Once());
            Verify_GetBetterInfo(Times.Never());
        }

        #endregion
        #region Getters
        private IBettingService GetService()
        {
            _bettingRepository = new Mock<IBettingRepository>();

            return new BettingService(_bettingRepository.Object);
        }

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

        private void SetupBettingRepository_DoesBetterExist_ReturnsTrue()
        {
            _bettingRepository.Setup(m => m.DoesBetterExist(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
        }

        private void SetupBettingRepository_DoesBetterExist_ReturnsFalse()
        {
            _bettingRepository.Setup(m => m.DoesBetterExist(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);
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

        private void Verify_RemoveMatch(Times times)
        {
            _bettingRepository.Verify(m => m.RemoveMatch(It.IsAny<string>(), It.IsAny<string>()), times);
        }

        private void Verify_DeclareMatchWinner(Times times)
        {
            _bettingRepository.Verify(
                m => m.DeclareMatchWinner(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), times);
        }

        private void Verify_GetMatchResult(Times times)
        {
            _bettingRepository.Verify(m => m.GetMatchResult(It.IsAny<string>(), It.IsAny<string>()), times);
        }

        private void Verify_DoesBetterExist(Times times)
        {
            _bettingRepository.Verify(m => m.DoesBetterExist(It.IsAny<string>(), It.IsAny<string>()), times);
        }

        private void Verify_GetBalance(Times times)
        {
            _bettingRepository.Verify(m => m.GetBalance(It.IsAny<string>(), It.IsAny<string>()), times);
        }

        private void Verify_GetAvailableMatches(Times times)
        {
            _bettingRepository.Verify(m => m.GetMatches(It.IsAny<string>()), times);
        }

        private void Verify_AddBet(Times times)
        {
            _bettingRepository.Verify(m => m.AddBet(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<int>()), times);
        }

        private void Verify_GetLeaderBoard(Times times)
        {
            _bettingRepository.Verify(m => m.GetLeaderBoard(It.IsAny<string>()), times);
        }

        private void Verify_GetBetterInfo(Times times)
        {
            _bettingRepository.Verify(m => m.GetBetterInfo(It.IsAny<string>(), It.IsAny<string>()), times);
        }

        #endregion
    }
}
