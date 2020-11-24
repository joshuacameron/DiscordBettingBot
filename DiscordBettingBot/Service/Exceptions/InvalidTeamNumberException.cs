using System;

namespace DiscordBettingBot.Service.Exceptions
{
    public class InvalidTeamNumberException : Exception
    {
        public InvalidTeamNumberException(int teamNumber)
            : base($"Team number \"{teamNumber}\" is invalid, options are 1 or 2") { }
    }
}
