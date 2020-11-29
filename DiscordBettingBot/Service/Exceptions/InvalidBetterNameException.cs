using System;

namespace DiscordBettingBot.Common.Service.Exceptions
{
    public class InvalidBetterNameException : Exception
    {
        public InvalidBetterNameException(string tournamentName)
            : base($"Better name \"{tournamentName}\" is invalid, must be at least 1 character and <= 254") { }
    }
}
