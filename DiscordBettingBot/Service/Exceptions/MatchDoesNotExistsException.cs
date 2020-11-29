using System;

namespace DiscordBettingBot.Common.Service.Exceptions
{
    public class MatchDoesNotExistsException : Exception
    {
        public MatchDoesNotExistsException() {}
        public MatchDoesNotExistsException(string matchName)
            : base($"Match \"{matchName}\" does not exist") { }
    }
}
