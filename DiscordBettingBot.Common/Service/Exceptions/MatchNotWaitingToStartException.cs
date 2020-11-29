using System;

namespace DiscordBettingBot.Common.Service.Exceptions
{
    public class MatchNotWaitingToStartException : Exception
    {
        public MatchNotWaitingToStartException(string matchName)
            : base($"Match \"{matchName}\" is not waiting to start") { }
    }
}
