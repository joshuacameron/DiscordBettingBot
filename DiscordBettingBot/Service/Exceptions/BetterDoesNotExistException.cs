using System;

namespace DiscordBettingBot.Service.Exceptions
{
    public class BetterDoesNotExistException : Exception
    {
        public BetterDoesNotExistException(string betterName)
            : base($"Better \"{betterName}\" does not exist") { }
    }
}
