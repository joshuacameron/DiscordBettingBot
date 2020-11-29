using System;

namespace DiscordBettingBot.Common.Service.Exceptions
{
    public class InvalidTournamentNameException : Exception
    {
        public InvalidTournamentNameException(string tournamentName)
            : base($"Tournament name \"{tournamentName}\" is invalid, must be at least 1 character and <= 254") { }
    }
}
