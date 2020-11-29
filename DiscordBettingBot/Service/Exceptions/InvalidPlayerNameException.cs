using System;

namespace DiscordBettingBot.Common.Service.Exceptions
{
    public class InvalidPlayerNameException : Exception
    {
        public InvalidPlayerNameException(string tournamentName)
            : base($"Player name \"{tournamentName}\" is invalid, must be at least 1 character and <= 254") { }
    }
}
