using Discord.Commands;
using DiscordBettingBot.Attributes;
using DiscordBettingBot.Common.Service.Interfaces;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBettingBot.Modules
{
    //TODO: Reply with the name of the person who asked you prepended
    //TODO: Imply OfficeTournament, using a SetTournament command
    //TODO: Fix reversing bet amount (we give the bet back, but we don't take the winnings if there was some)
    //TODO: Add tests
    //TODO: Add an info which says commands
    //TODO: Add a ? value after each command to get how to run it
    public class BettingModule : ModuleBase<SocketCommandContext>
    {
        private const ulong AdministratorRoleId = 723043693199884300;
        private readonly IBettingService _bettingService;
        
        public BettingModule(IBettingService bettingService)
        {
            _bettingService = bettingService;
        }

        [Command("Truncate")]
        [RequireRole(AdministratorRoleId)]
        public Task Truncate()
        {
            return PerformAction(() =>
            {
                _bettingService.TruncateDatabase();
                return "Database truncated";
            });
        }

        [Command("StartTournament")]
        [RequireRole(AdministratorRoleId)]
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
        [RequireRole(AdministratorRoleId)]
        public Task AddMatch(params string[] commandParams)
        {
            if (commandParams == null || commandParams.Length < 2)
            {
                ReplyAsync($"Incorrect arguments given, needed at least 2, given {commandParams?.Length ?? 0}");
                return Task.FromResult<object>(null);
            }

            if (commandParams.Length != 2 && commandParams.Length % 2 != 0)
            {
                ReplyAsync("Incorrect arguments given, needed at least 2, plus balanced teams");
                return Task.FromResult<object>(null);
            }

            return PerformAction(() =>
            {
                var players = commandParams.ToList().GetRange(2, commandParams.Length - 2).ToList();
                var team1 = players.GetRange(0, players.Count / 2);
                var team2 = players.GetRange(players.Count / 2, players.Count / 2);

                _bettingService.AddMatch(commandParams[0], commandParams[1], team1.ToArray(), team2.ToArray());
                return $"Added new match in tournament \"{commandParams[0]}\" called \"{commandParams[1]}\"";
            });
        }

        [Command("StartMatch")]
        [RequireRole(AdministratorRoleId)]
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

        [Command("RemoveMatch")]
        [RequireRole(AdministratorRoleId)]
        public Task RemoveMatch(params string[] commandParams)
        {
            if (!ConfirmCorrectNumberParams(commandParams, 2))
            {
                return Task.FromResult<object>(null);
            }

            return PerformAction(() =>
            {
                _bettingService.RemoveMatch(commandParams[0], commandParams[1]);
                return $"Removed match from tournament \"{commandParams[0]}\" called \"{commandParams[1]}\"";
            });
        }

        [Command("AddBetter")]
        [RequireRole(AdministratorRoleId)]
        public Task AddBetter(params string[] commandParams)
        {
            if (!ConfirmCorrectNumberParams(commandParams, 3))
            {
                return Task.FromResult<object>(null);
            }
            
            return PerformAction(() =>
            {
                var initialBalance = decimal.Parse(commandParams[2]);
                _bettingService.AddBetter(commandParams[0], commandParams[1], initialBalance);
                return $"Added better \"{commandParams[1]}\" to tournament \"{commandParams[0]}\" with initial balance of \"{initialBalance}\"";
            });
        }

        [Command("AddBet")]
        public Task AddBet(params string[] commandParams)
        {
            if (!ConfirmCorrectNumberParams(commandParams, 5))
            {
                return Task.FromResult<object>(null);
            }

            return PerformAction(() =>
            {
                var betAmount = decimal.Parse(commandParams[3]);
                var teamNumber = int.Parse(commandParams[4]);
                var players = string.Join(',', _bettingService.GetPlayersByMatch(commandParams[0], commandParams[2]).Select(x => x.Name.Any(char.IsWhiteSpace) ? "\"" + x.Name + "\"" : x.Name));

                _bettingService.AddBet(commandParams[0], commandParams[1], commandParams[2], betAmount, teamNumber);
                return $"Added \"{betAmount}\" bet from better \"{commandParams[1]}\" to tournament \"{commandParams[0]}\" match {commandParams[2]} for team {teamNumber} ({players})";
            });
        }

        [Command("Matches")]
        public Task GetMatches(params string[] commandParams)
        {
            if (!ConfirmCorrectNumberParams(commandParams, 1))
            {
                return Task.FromResult<object>(null);
            }

            return PerformAction(() =>
            {
                var matches = _bettingService.GetMatches(commandParams[0]).ToList();

                if (!matches.Any())
                {
                    return $"There are no matches for tournament \"{commandParams[0]}\"";
                }

                var reply = new StringBuilder();
                reply.AppendLine($"Current matches for tournament \"{commandParams[0]}\":");

                foreach (var match in matches)
                {
                    var name = match.Name;
                    var status = match.Status.ToString();
                    var players1 = string.Join(',', match.Team1.Select(x => x.Name.Any(char.IsWhiteSpace) ? "\"" + x.Name + "\"" : x.Name));
                    var players2 = string.Join(',', match.Team2.Select(x => x.Name.Any(char.IsWhiteSpace) ? "\"" + x.Name + "\"" : x.Name));
                    var winningTeamNumber = match.WinningTeamNumber == null ? "N/A" : match.WinningTeamNumber.ToString();

                    reply.AppendLine($"Name: {name},\tStatus:{status},\tTeam1: {players1},\tTeam2: {players2},\tWinningTeam: {winningTeamNumber}");
                }

                return reply.ToString();
            });
        }

        [Command("Balance")]
        public Task GetBetterInfo(params string[] commandParams)
        {
            if (!ConfirmCorrectNumberParams(commandParams, 2))
            {
                return Task.FromResult<object>(null);
            }

            return PerformAction(() =>
            {
                var betterInfo = _bettingService.GetBetterInfo(commandParams[0], commandParams[1]);

                return $"Better \"{betterInfo.Name}\" has balance of {betterInfo.Balance}. They have won {betterInfo.WonBetsCount} bets, lost {betterInfo.LostBetsCount} bets and have {betterInfo.OutstandingBetsCount} bets outstanding.";
            });
        }

        [Command("Leaderboard")]
        public Task LeaderBoard(params string[] commandParams)
        {
            if (!ConfirmCorrectNumberParams(commandParams, 1))
            {
                return Task.FromResult<object>(null);
            }

            return PerformAction(() =>
            {
                var betters = _bettingService.GetLeaderBoard(commandParams[0]).ToList();

                var reply = new StringBuilder();

                reply.AppendLine("Current leaderboard:");

                for(int i = 1; i <= betters.Count; i++)
                {
                    var better = betters[i - 1];
                    reply.AppendLine(
                        i + $". Better \"{better.Name}\" has balance of {better.Balance}. They have won {better.WonBetsCount} bets, lost {better.LostBetsCount} bets and have {better.OutstandingBetsCount} bets outstanding.");
                }

                return reply.ToString();
            });
        }

        [Command("DeclareMatchWinner")]
        [RequireRole(AdministratorRoleId)]
        public Task DeclareMatchWinner(params string[] commandParams)
        {
            if (!ConfirmCorrectNumberParams(commandParams, 3))
            {
                return Task.FromResult<object>(null);
            }

            return PerformAction(() =>
            {
                var match = _bettingService.DeclareMatchWinner(commandParams[0], commandParams[1], int.Parse(commandParams[2]));

                var winningBets = match.Bets.Where(x => x.Won == true).ToList();
                var losingBets = match.Bets.Where(x => x.Won == false).ToList();

                var winningBetterIds = winningBets.Select(x => x.BetterId).Distinct();
                var losingBetterIds = losingBets.Select(x => x.BetterId).Distinct();

                var winningBetters = match.MatchBetters.Where(x => winningBetterIds.Contains(x.Id)).ToList();
                var losingBetters = match.MatchBetters.Where(x => losingBetterIds.Contains(x.Id)).ToList();

                //TODO: Players aren't loading
                var players = string.Join(',',
                    match.WinningPlayers.Select(x => x.Any(char.IsWhiteSpace) ? "\"" + x + "\"" : x));

                var reply = new StringBuilder();
                reply.AppendLine($"Congratulations to {players} for winning match \"{match.MatchName}\"");

                if (winningBetters.Any())
                {
                    reply.AppendLine("Bet winners:");
                    foreach (var winningBetter in winningBetters)
                    {
                        var betTotal = winningBets.Where(x => x.BetterId == winningBetter.Id).Sum(x => x.Amount);
                        reply.AppendLine($"Better {winningBetter.Name} bet a total of {betTotal} and won {betTotal * 2}, new balance: {winningBetter.Balance}");
                    }
                }

                if (winningBetters.Any() && losingBetters.Any())
                {
                    reply.AppendLine();
                }

                if (losingBetters.Any())
                {
                    reply.AppendLine("Bet losers:");
                    foreach (var losingBetter in losingBetters)
                    {
                        var betTotal = winningBets.Where(x => x.BetterId == losingBetter.Id).Sum(x => x.Amount);
                        reply.AppendLine($"Better {losingBetter.Name} bet and lost a total of {betTotal}, new balance: {losingBetter.Balance}");
                    }
                }

                return reply.ToString();
            });
        }

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
