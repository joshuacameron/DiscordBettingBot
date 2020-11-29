using System;

namespace DiscordBettingBot.Common.Service.Exceptions
{
    public class BetterDoesNotExistException : Exception
    {
        public BetterDoesNotExistException(string betterName)
            : base($"Better \"{betterName}\" does not exist") { }
    }
}
