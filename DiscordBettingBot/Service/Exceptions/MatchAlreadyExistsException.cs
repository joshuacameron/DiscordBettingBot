using System;

namespace DiscordBettingBot.Service.Exceptions
{
    public class MatchAlreadyExistsException : Exception
    {
        public MatchAlreadyExistsException(string matchName)
            : base($"Match \"{matchName}\" already exists") { }
    }
}
