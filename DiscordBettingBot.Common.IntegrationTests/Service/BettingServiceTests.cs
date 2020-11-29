using Autofac;
using DiscordBettingBot.Common.Data.Interfaces;
using DiscordBettingBot.Common.Data.Models;
using DiscordBettingBot.Common.Service;
using DiscordBettingBot.Common.Service.Enumerations;
using DiscordBettingBot.Common.Service.Exceptions;
using DiscordBettingBot.Common.Service.Interfaces;
using NUnit.Framework;
using System.Linq;

namespace DiscordBettingBot.Common.IntegrationTests.Service
{
    public class BettingServiceTests : IntegrationTestBase
    {
        private IBettingRepository _bettingRepository;
        private IBettingService _bettingService;

        private const string ValidTournamentName = "TournamentName";
        private const string InvalidTournamentName = "";
        private const string ValidMatchName = "MatchName";
        private const string InvalidMatchName = "";
        private const string ValidBetterName = "BetterName";
        private const string ValidBetterName2 = "BetterName2";
        private const string InvalidBetterName = "";
        private const int ValidTeamNumber = 1;
        private const int InvalidTeamNumber = -1;
        private const int ValidEnemyTeamNumber = 2;
        private const int ValidBetAmount = 1;
        private const int InvalidBetAmount = -1;
        private const decimal ValidInitialBalance = 100;

        private static string[] ValidTeam1Players => new [] { "Player1", "Player2" };
        private static string[] ValidTeam2Players => new[] { "Player3", "Player4" };

        private static string[] InvalidTeam1Players => new[] { "", "" };
        private static string[] InvalidTeam2Players => new[] { "", "" };

        [SetUp]
        public void SetupBeforeEachTest()
        {
            _bettingRepository = Container.Resolve<IBettingRepository>();
            _bettingService = new BettingService(_bettingRepository);
        }

        [Test]
        public void StartNewTournament_When_TournamentNotExist_Should_StartNewTournament()
        {
            var doesExistBefore = DoesTournamentExist(ValidTournamentName);
            _bettingService.StartNewTournament(ValidTournamentName);
            var doesExistAfter = DoesTournamentExist(ValidTournamentName);
            
            Assert.False(doesExistBefore);
            Assert.True(doesExistAfter);
        }

        [Test]
        public void StartNewTournament_When_TournamentExists_Should_ThrowException()
        {
            _bettingService.StartNewTournament(ValidTournamentName);

            Assert.Throws<TournamentAlreadyExistsException>(() => _bettingService.StartNewTournament(ValidTournamentName));
        }

        [Test]
        public void StartNewTournament_When_InvalidTournamentName_Should_ThrowException()
        {
            Assert.Throws<InvalidTournamentNameException>(() => _bettingService.StartNewTournament(InvalidTournamentName));
        }

        [Test]
        public void StartMatch_When_HappyPath_Should_StartMatch()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);
            _bettingService.StartMatch(ValidTournamentName, ValidMatchName);

            var match = GetMatch(ValidTournamentName, ValidMatchName);

            Assert.True(match.Status == MatchStatus.Running);
        }

        [Test]
        public void StartMatch_When_InvalidTournamentName_Should_ThrowException()
        {
            Assert.Throws<InvalidTournamentNameException>(() => _bettingService.StartMatch(InvalidTournamentName, ValidMatchName));
        }

        [Test]
        public void StartMatch_When_InvalidMatchName_Should_ThrowException()
        {
            Assert.Throws<InvalidMatchNameException>(() => _bettingService.StartMatch(ValidTournamentName, InvalidMatchName));
        }

        [Test]
        public void StartMatch_When_TournamentDoesNotExist_Should_ThrowException()
        {
            Assert.Throws<TournamentDoesNotExistException>(() => _bettingService.StartMatch(ValidTournamentName, ValidMatchName));
        }

        [Test]
        public void StartMatch_When_MatchDoesNotExist_Should_ThrowException()
        {
            _bettingRepository.InsertTournament(ValidTournamentName);
            Assert.Throws<MatchDoesNotExistsException>(() => _bettingService.StartMatch(ValidTournamentName, ValidMatchName));
        }

        [Test]
        public void StartMatch_When_MatchFinished_Should_ThrowException()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);
            _bettingService.StartMatch(ValidTournamentName, ValidMatchName);
            _bettingService.DeclareMatchWinner(ValidTournamentName, ValidMatchName, ValidTeamNumber);

            Assert.Throws<MatchNotWaitingToStartException>(() => _bettingService.StartMatch(ValidTournamentName, ValidMatchName));
        }

        [Test]
        public void StartMatch_When_MatchRunning_Should_ThrowException()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);
            _bettingService.StartMatch(ValidTournamentName, ValidMatchName);

            Assert.Throws<MatchNotWaitingToStartException>(() => _bettingService.StartMatch(ValidTournamentName, ValidMatchName));
        }
        
        [Test]
        public void RemoveMatch_When_HappyPath_Should_RemoveMatch()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);

            var match = _bettingService.GetMatches(ValidTournamentName).First();
            var playersBefore = _bettingRepository.GetPlayerByMatchId(match.Id);

            var betsBefore = _bettingRepository.GetBetsByMatchId(match.Id);

            const int initialBalance = 100;
            const int betAmount = 10;

            _bettingService.AddBetter(ValidTournamentName, ValidBetterName, initialBalance);

            var betterBefore = _bettingService.GetBetterInfo(ValidTournamentName, ValidBetterName);

            _bettingService.AddBet(ValidTournamentName, ValidBetterName, ValidMatchName, betAmount, 1);

            var betterAfter = _bettingService.GetBetterInfo(ValidTournamentName, ValidBetterName);

            _bettingService.RemoveMatch(ValidTournamentName, ValidMatchName);

            var betterRefunded = _bettingService.GetBetterInfo(ValidTournamentName, ValidBetterName);

            var playersAfter = _bettingRepository.GetPlayerByMatchId(match.Id);
            var betsAfter = _bettingRepository.GetBetsByMatchId(match.Id);

            Assert.AreEqual(initialBalance, betterBefore.Balance);
            Assert.AreEqual(initialBalance - betAmount, betterAfter.Balance);
            Assert.AreEqual(initialBalance, betterRefunded.Balance);
            Assert.IsTrue(betsBefore.Count == 0);
            Assert.AreEqual(ValidTeam1Players.Length + ValidTeam2Players.Length, playersBefore.Count);
            Assert.IsEmpty(playersAfter);
            Assert.IsEmpty(betsAfter);
        }

        [Test]
        public void RemoveMatch_When_InvalidTournamentName_Should_ThrowException()
        {
            Assert.Throws<InvalidTournamentNameException>(() => _bettingService.RemoveMatch(InvalidTournamentName, ValidMatchName));
        }

        [Test]
        public void RemoveMatch_When_InvalidMatchName_Should_ThrowException()
        {
            Assert.Throws<InvalidMatchNameException>(() => _bettingService.RemoveMatch(ValidTournamentName, InvalidMatchName));
        }

        [Test]
        public void RemoveMatch_When_TournamentDoesNotExist_Should_ThrowException()
        {
            Assert.Throws<TournamentDoesNotExistException>(() => _bettingService.RemoveMatch(ValidTournamentName, ValidMatchName));
        }

        [Test]
        public void RemoveMatch_When_MatchDoesNotExist_Should_ThrowException()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            Assert.Throws<MatchDoesNotExistsException>(() => _bettingService.RemoveMatch(ValidTournamentName, ValidMatchName));
        }

        [Test]
        public void GetBalance_When_Created_Should_GetInitialBalance()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddBetter(ValidTournamentName, ValidBetterName, ValidInitialBalance);

            var balance = _bettingService.GetBalance(ValidTournamentName, ValidBetterName);

            Assert.AreEqual(ValidInitialBalance, balance);
        }

        [Test]
        public void GetBalance_When_CreatedPlusWin_Should_GetNewBalance()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);
            _bettingService.AddBetter(ValidTournamentName, ValidBetterName, ValidInitialBalance);
            _bettingService.AddBet(ValidTournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber);
            _bettingService.StartMatch(ValidTournamentName, ValidMatchName);
            _bettingService.DeclareMatchWinner(ValidTournamentName, ValidMatchName, ValidTeamNumber);

            var balance = _bettingService.GetBalance(ValidTournamentName, ValidBetterName);

            Assert.AreEqual(ValidInitialBalance - ValidBetAmount + 2 * ValidBetAmount, balance);
        }

        [Test]
        public void GetBalance_When_CreatedPlusLoss_Should_GetNewBalance()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);
            _bettingService.AddBetter(ValidTournamentName, ValidBetterName, ValidInitialBalance);
            _bettingService.AddBet(ValidTournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidEnemyTeamNumber);
            _bettingService.StartMatch(ValidTournamentName, ValidMatchName);
            _bettingService.DeclareMatchWinner(ValidTournamentName, ValidMatchName, ValidTeamNumber);

            var balance = _bettingService.GetBalance(ValidTournamentName, ValidBetterName);

            Assert.AreEqual(ValidInitialBalance - ValidBetAmount, balance);
        }

        [Test]
        public void GetBalance_When_InvalidTournamentName_Should_ThrowException()
        {
            Assert.Throws<InvalidTournamentNameException>(() => _bettingService.GetBalance(InvalidTournamentName, ValidBetterName));
        }

        [Test]
        public void GetBalance_When_InvalidBetterName_Should_ThrowException()
        {
            Assert.Throws<InvalidBetterNameException>(() => _bettingService.GetBalance(ValidTournamentName, InvalidBetterName));
        }

        [Test]
        public void GetBalance_When_TournamentDoesNotExist_Should_ThrowException()
        {
            Assert.Throws<TournamentDoesNotExistException>(() => _bettingService.GetBalance(ValidTournamentName, ValidBetterName));
        }

        [Test]
        public void GetBalance_When_BetterDoesNotExist_Should_ThrowException()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            Assert.Throws<BetterDoesNotExistException>(() => _bettingService.GetBalance(ValidTournamentName, ValidBetterName));
        }

        [Test]
        public void GetLeaderBoard_When_HappyPath_Should_GetLeaderBoard()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);
            _bettingService.AddBetter(ValidTournamentName, ValidBetterName, ValidInitialBalance);
            _bettingService.AddBetter(ValidTournamentName, ValidBetterName2, ValidInitialBalance);
            _bettingService.AddBet(ValidTournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber);
            _bettingService.AddBet(ValidTournamentName, ValidBetterName2, ValidMatchName, ValidBetAmount, ValidEnemyTeamNumber);
            _bettingService.StartMatch(ValidTournamentName, ValidMatchName);
            _bettingService.DeclareMatchWinner(ValidTournamentName, ValidMatchName, ValidTeamNumber);

           var betters = _bettingService.GetLeaderBoard(ValidTournamentName).ToList();

           Assert.AreEqual(2, betters.Count);

           Assert.AreEqual(ValidBetterName, betters[0].Name);
           Assert.AreEqual(ValidBetterName2, betters[1].Name);
        }

        [Test]
        public void GetLeaderBoard_When_InvalidTournamentName_Should_ThrowException()
        {
            Assert.Throws<InvalidTournamentNameException>(() => _bettingService.GetLeaderBoard(InvalidTournamentName));
        }

        [Test]
        public void GetLeaderBoard_When_TournamentDoesNotExist_Should_ThrowException()
        {
            Assert.Throws<TournamentDoesNotExistException>(() => _bettingService.GetLeaderBoard(ValidTournamentName));
        }

        [Test]
        public void GetBetterInfo_When_InitiallySetup_Should_GetBetterInfoWithInitialAmount()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddBetter(ValidTournamentName, ValidBetterName, ValidInitialBalance);

            var betterInfo = _bettingService.GetBetterInfo(ValidTournamentName, ValidBetterName);

            Assert.AreEqual(ValidInitialBalance, betterInfo.Balance);
        }

        [Test]
        public void GetBetterInfo_When_CreatedPlusWin_Should_GetInfoWithNewBalance()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);
            _bettingService.AddBetter(ValidTournamentName, ValidBetterName, ValidInitialBalance);
            _bettingService.AddBet(ValidTournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber);
            _bettingService.StartMatch(ValidTournamentName, ValidMatchName);
            _bettingService.DeclareMatchWinner(ValidTournamentName, ValidMatchName, ValidTeamNumber);

            var betterInfo = _bettingService.GetBetterInfo(ValidTournamentName, ValidBetterName);

            Assert.AreEqual(ValidInitialBalance - ValidBetAmount + 2 * ValidBetAmount, betterInfo.Balance);
        }

        [Test]
        public void GetBetterInfo_When_CreatedPlusLoss_Should_GetInfoWithNewBalance()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);
            _bettingService.AddBetter(ValidTournamentName, ValidBetterName, ValidInitialBalance);
            _bettingService.AddBet(ValidTournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidEnemyTeamNumber);
            _bettingService.StartMatch(ValidTournamentName, ValidMatchName);
            _bettingService.DeclareMatchWinner(ValidTournamentName, ValidMatchName, ValidTeamNumber);

            var betterInfo = _bettingService.GetBetterInfo(ValidTournamentName, ValidBetterName);

            Assert.AreEqual(ValidInitialBalance - ValidBetAmount, betterInfo.Balance);
        }

        [Test]
        public void GetBetterInfo_When_InvalidTournamentName_Should_ThrowException()
        {
            Assert.Throws<InvalidTournamentNameException>(() => _bettingService.GetBetterInfo(InvalidTournamentName, ValidBetterName));
        }

        [Test]
        public void GetBetterInfo_When_InvalidBetterName_Should_ThrowException()
        {
            Assert.Throws<InvalidBetterNameException>(() => _bettingService.GetBetterInfo(ValidTournamentName, InvalidBetterName));
        }

        [Test]
        public void GetBetterInfo_When_TournamentDoesNotExist_Should_ThrowException()
        {
            Assert.Throws<TournamentDoesNotExistException>(() => _bettingService.GetBetterInfo(ValidTournamentName, ValidBetterName));
        }

        [Test]
        public void GetBetterInfo_When_BetterDoesNotExist_Should_ThrowException()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            Assert.Throws<BetterDoesNotExistException>(() => _bettingService.GetBetterInfo(ValidTournamentName, ValidBetterName));
        }

        [Test]
        public void AddMatch_When_HappyPath_Should_AddMatch()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);

            var match = _bettingService.GetMatches(ValidTournamentName).ToList();

            Assert.AreEqual(1, match.Count);
            Assert.AreEqual(ValidMatchName, match.First().Name);
        }

        [Test]
        public void AddMatch_When_InvalidTournamentName_Should_ThrowException()
        {
            Assert.Throws<InvalidTournamentNameException>(() =>
                _bettingService.AddMatch(InvalidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players));
        }

        [Test]
        public void AddMatch_When_InvalidMatchName_Should_ThrowException()
        {
            Assert.Throws<InvalidMatchNameException>(() =>
                _bettingService.AddMatch(ValidTournamentName, InvalidMatchName, ValidTeam1Players, ValidTeam2Players));
        }

        [Test]
        public void AddMatch_When_InvalidPlayerNameTeam1_Should_ThrowException()
        {
            Assert.Throws<InvalidPlayerNameException>(() =>
                _bettingService.AddMatch(ValidTournamentName, ValidMatchName, InvalidTeam1Players, ValidTeam2Players));
        }

        [Test]
        public void AddMatch_When_InvalidPlayerNameTeam2_Should_ThrowException()
        {
            Assert.Throws<InvalidPlayerNameException>(() =>
                _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, InvalidTeam2Players));
        }

        [Test]
        public void AddMatch_When_TournamentDoesNotExist_Should_ThrowException()
        {
            Assert.Throws<TournamentDoesNotExistException>(() =>
                _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players));
        }

        [Test]
        public void AddMatch_When_MatchAlreadyExists_Should_ThrowException()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);

            Assert.Throws<MatchAlreadyExistsException>(() =>
                _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players));
        }

        [Test]
        public void GetMatches_When_NoMatchesInTournament_Should_ReturnEmpty()
        {
            _bettingService.StartNewTournament(ValidTournamentName);

            var matches = _bettingService.GetMatches(ValidTournamentName);

            Assert.AreEqual(0, matches.Count());
        }

        [Test]
        public void GetMatches_When_OneMatchInTournament_Should_ReturnOne()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);

            var matches = _bettingService.GetMatches(ValidTournamentName);

            Assert.AreEqual(1, matches.Count());
        }

        [Test]
        public void GetMatches_When_InvalidTournamentName_Should_ThrowException()
        {
            Assert.Throws<InvalidTournamentNameException>(() => _bettingService.GetMatches(InvalidTournamentName));
        }

        [Test]
        public void GetMatches_When_TournamentDoesNotExist_Should_ThrowException()
        {
            Assert.Throws<TournamentDoesNotExistException>(() => _bettingService.GetMatches(ValidTournamentName));
        }

        [Test]
        public void AddBet_When_HappyPath_Should_AddBet()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddBetter(ValidTournamentName, ValidBetterName, ValidInitialBalance);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);
            _bettingService.AddBet(ValidTournamentName, ValidBetterName, ValidMatchName, ValidBetAmount,
                ValidTeamNumber);

            var bets = _bettingService.GetBetterInfo(ValidTournamentName, ValidBetterName).Bets.ToList();

            Assert.AreEqual(1, bets.Count);
            Assert.AreEqual(ValidBetAmount, bets.First().Amount);
        }

        [Test]
        public void AddBet_When_InvalidTournamentName_Should_ThrowException()
        {
            Assert.Throws<InvalidTournamentNameException>(() =>
                _bettingService.AddBet(InvalidTournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber));
        }

        [Test]
        public void AddBet_When_InvalidBetterName_Should_ThrowException()
        {
            Assert.Throws<InvalidBetterNameException>(() =>
                _bettingService.AddBet(ValidTournamentName, InvalidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber));
        }

        [Test]
        public void AddBet_When_InvalidMatchName_Should_ThrowException()
        {
            Assert.Throws<InvalidMatchNameException>(() =>
                _bettingService.AddBet(ValidTournamentName, ValidBetterName, InvalidMatchName, ValidBetAmount, ValidTeamNumber));
        }

        [Test]
        public void AddBet_When_InvalidBetAmount_Should_ThrowException()
        {
            Assert.Throws<InvalidBetAmountException>(() =>
                _bettingService.AddBet(ValidTournamentName, ValidBetterName, ValidMatchName, InvalidBetAmount, ValidTeamNumber));
        }

        [Test]
        public void AddBet_When_InvalidTeamNumber_Should_ThrowException()
        {
            Assert.Throws<InvalidTeamNumberException>(() =>
                _bettingService.AddBet(ValidTournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, InvalidTeamNumber));
        }

        [Test]
        public void AddBet_When_TournamentDoesNotExist_Should_ThrowException()
        {
            Assert.Throws<TournamentDoesNotExistException>(() =>
                _bettingService.AddBet(ValidTournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber));
        }

        [Test]
        public void AddBet_When_MatchDoesNotExist_Should_ThrowException()
        {
            _bettingService.StartNewTournament(ValidTournamentName);

            Assert.Throws<MatchDoesNotExistsException>(() =>
                _bettingService.AddBet(ValidTournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber));
        }

        [Test]
        public void AddBet_When_MatchNotWaitingToStart_Should_ThrowException()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddBetter(ValidTournamentName, ValidBetterName, ValidInitialBalance);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);
            _bettingService.StartMatch(ValidTournamentName, ValidMatchName);

            Assert.Throws<MatchNotWaitingToStartException>(() =>
                _bettingService.AddBet(ValidTournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber));
        }
        
        [Test]
        public void DeclareMatchWinner_When_HappyPath_Should_DeclareMatchWinner()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            _bettingService.AddMatch(ValidTournamentName, ValidMatchName, ValidTeam1Players, ValidTeam2Players);
            _bettingService.AddBetter(ValidTournamentName, ValidBetterName, ValidInitialBalance);
            _bettingService.AddBet(ValidTournamentName, ValidBetterName, ValidMatchName, ValidBetAmount, ValidTeamNumber);

            _bettingService.DeclareMatchWinner(ValidTournamentName, ValidMatchName, ValidTeamNumber);

            var status = _bettingService.GetMatches(ValidTournamentName).First().Status;
            var betterBalance = _bettingService.GetBetterInfo(ValidTournamentName, ValidBetterName).Balance;

            Assert.AreEqual(MatchStatus.Finished, status);
            Assert.AreEqual(ValidInitialBalance + ValidBetAmount, betterBalance);
        }

        [Test]
        public void DeclareMatchWinner_When_InvalidTournamentName_Should_ThrowException()
        {
            Assert.Throws<InvalidTournamentNameException>(() =>
                _bettingService.DeclareMatchWinner(InvalidTournamentName, ValidMatchName, ValidTeamNumber));
        }

        [Test]
        public void DeclareMatchWinner_When_InvalidMatchName_Should_ThrowException()
        {
            Assert.Throws<InvalidMatchNameException>(() =>
                _bettingService.DeclareMatchWinner(ValidTournamentName, InvalidMatchName, ValidTeamNumber));
        }

        [Test]
        public void DeclareMatchWinner_When_InvalidTeamNumber_Should_ThrowException()
        {
            Assert.Throws<InvalidTeamNumberException>(() =>
                _bettingService.DeclareMatchWinner(ValidTournamentName, ValidMatchName, InvalidTeamNumber));
        }

        [Test]
        public void DeclareMatchWinner_When_TournamentDoesNotExist_Should_ThrowException()
        {
            Assert.Throws<TournamentDoesNotExistException>(() =>
                _bettingService.DeclareMatchWinner(ValidTournamentName, ValidMatchName, ValidTeamNumber));
        }

        [Test]
        public void DeclareMatchWinner_When_MatchDoesNotExist_Should_ThrowException()
        {
            _bettingService.StartNewTournament(ValidTournamentName);
            Assert.Throws<MatchDoesNotExistsException>(() =>
                _bettingService.DeclareMatchWinner(ValidTournamentName, ValidMatchName, ValidTeamNumber));
        }

        #region Helpers

        private bool DoesTournamentExist(string tournamentName)
        {
            return _bettingRepository.GetTournamentByName(tournamentName) != null;
        }

        private Match GetMatch(string tournamentName, string matchName)
        {
            var tournament = _bettingRepository.GetTournamentByName(tournamentName);
            return _bettingRepository.GetMatchByName(tournament.Id, matchName);
        }

        #endregion
    }
}
