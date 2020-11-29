using System;
using Discord.Commands;
using System.Threading.Tasks;
using DiscordBettingBot.Common.Service.Interfaces;

namespace DiscordBettingBot.Modules
{
    public class BettingModule : ModuleBase<SocketCommandContext>
    {
        private readonly IBettingService _bettingService;

        public BettingModule(IBettingService bettingService)
        {
            _bettingService = bettingService;
        }

        [Command("StartTournament")]
        public Task StartTournament(params string[] commandParams)
        {
            if (!ConfirmCorrectNumberParams(commandParams, 1))
            {
                return Task.FromResult<object>(null);
            }

            return PerformAction(() =>
            {
                _bettingService.StartNewTournament(commandParams[0]);
                return $"Started new tournament \"{commandParams[0]}\"";
            });
        }

        [Command("AddMatch")]
        public Task AddMatch(params string[] commandParams)
        {
            if (!ConfirmCorrectNumberParams(commandParams, 2))
            {
                return Task.FromResult<object>(null);
            }

            return PerformAction(() =>
            {
                _bettingService.AddMatch(commandParams[0], commandParams[1], new [] {"Player1"}, new []{"Player2"} );
                return $"Added new match in tournament \"{commandParams[0]}\" called \"{commandParams[1]}\"";
            });
        }

        [Command("StartMatch")]
        public Task StartMatch(params string[] commandParams)
        {
            if (!ConfirmCorrectNumberParams(commandParams, 2))
            {
                return Task.FromResult<object>(null);
            }

            return PerformAction(() =>
            {
                _bettingService.StartMatch(commandParams[0], commandParams[1]);
                return $"Started new match in tournament \"{commandParams[0]}\" called \"{commandParams[1]}\"";
            });
        }


        /*
         *        void AddMatch(string tournamentName, string matchName, string[] team1, string[] team2);
        void StartMatch(string tournamentName, string matchName);
        void RemoveMatch(string tournamentName, string matchName);
        MatchResult DeclareMatchWinner(string tournamentName, string matchName, int teamNumber);
        decimal GetBalance(string tournamentName, string betterName);
        IEnumerable<Match> GetMatches(string tournamentName);
        void AddBet(string tournamentName, string betterName, string matchName, decimal betAmount, int teamNumber);
        IEnumerable<Better> GetLeaderBoard(string tournamentName);
        Better GetBetterInfo(string tournamentName, string betterName);
        void AddBetter(string tournamentName, string betterName, decimal initialBalance);
        void StartNewTournament(string tournamentName);
         *
         *
         */

        private bool ConfirmCorrectNumberParams(string[] commandParams, int intendedNumber)
        {
            if (commandParams != null && commandParams.Length == intendedNumber) return true;

            ReplyAsync($"Incorrect arguments given, needed {intendedNumber}, given {commandParams?.Length ?? 0}");

            return false;
        }

        private Task PerformAction(Func<string> func)
        {
            try
            {
                var result = func.Invoke();
                return ReplyAsync(result);
            }
            catch (Exception e)
            {
                ReplyAsync($"Exception type: \"{e.GetType().Name}\", message: \"{e.Message}\"");
                return Task.FromResult<object>(null);
            }
        }
    }
}
