using System;

namespace DiscordBettingBot.Common.Data.Exceptions
{
    public class MatchNotFinishedException : Exception
    {
        public MatchNotFinishedException(string matchName)
            : base($"Match name \"{matchName}\" is not finished") { }
    }
}
