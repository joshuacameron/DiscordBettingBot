using System;

namespace DiscordBettingBot.Service.Exceptions
{
    public class InvalidTeamNumber : Exception
    {
        public InvalidTeamNumber(int teamNumber)
            : base($"Team number \"{teamNumber}\" is invalid, options are 1 or 2") { }
    }
}
