﻿using System;

namespace DiscordBettingBot.Common.Service.Exceptions
{
    public class TournamentAlreadyExistsException : Exception
    {
        public TournamentAlreadyExistsException(string tournamentName)
            : base($"Tournament \"{tournamentName}\" already exists") { }
    }
}
