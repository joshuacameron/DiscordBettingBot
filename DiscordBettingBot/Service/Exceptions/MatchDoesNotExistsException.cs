using System;

namespace DiscordBettingBot.Service.Exceptions
{
    public class MatchDoesNotExistsException : Exception
    {
        public MatchDoesNotExistsException(string matchName)
            : base($"Match \"{matchName}\" does not exist") { }
    }
}
