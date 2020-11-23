using System;

namespace DiscordBettingBot.Service.Exceptions
{
    public class TournamentDoesNotExistException : Exception
    {
        public TournamentDoesNotExistException(string tournamentName)
            : base($"Tournament \"{tournamentName}\" does not exist") { }
    }
}
