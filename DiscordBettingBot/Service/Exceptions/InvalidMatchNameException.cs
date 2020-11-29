using System;

namespace DiscordBettingBot.Common.Service.Exceptions
{
    public class InvalidMatchNameException : Exception
    {
        public InvalidMatchNameException(string tournamentName)
            : base($"Match name \"{tournamentName}\" is invalid, must be at least 1 character and <= 254") { }
    }
}
