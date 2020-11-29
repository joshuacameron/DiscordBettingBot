using System;

namespace DiscordBettingBot.Common.Service.Exceptions
{
    public class MatchNotRunningException : Exception
    {
        public MatchNotRunningException(string message)
            : base(message) { }
    }
}
