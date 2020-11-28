using System;

namespace DiscordBettingBot.Service.Exceptions
{
    public class TournamentDoesNotExistException : Exception
    {
        public TournamentDoesNotExistException(string tournamentName)
            : base($"Tournament with name \"{tournamentName}\" does not exist") { }

        public TournamentDoesNotExistException(long tournamentId)
            : base($"Tournament with Id \"{tournamentId}\" does not exist") { }
    }
}
