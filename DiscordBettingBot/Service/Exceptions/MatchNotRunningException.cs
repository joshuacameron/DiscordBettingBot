using System;

namespace DiscordBettingBot.Service.Exceptions
{
    public class MatchNotRunningException : Exception
    {
        public MatchNotRunningException(string message)
            : base(message) { }
    }
}
