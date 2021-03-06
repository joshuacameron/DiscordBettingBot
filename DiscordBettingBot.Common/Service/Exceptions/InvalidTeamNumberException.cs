﻿using System;

namespace DiscordBettingBot.Common.Service.Exceptions
{
    public class InvalidTeamNumberException : Exception
    {
        public InvalidTeamNumberException(int teamNumber)
            : base($"Team number \"{teamNumber}\" is invalid, options are 1 or 2") { }
    }
}
