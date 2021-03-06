﻿using System;

namespace DiscordBettingBot.Common.Service.Exceptions
{
    public class InvalidInitialAmountException : Exception
    {
        public InvalidInitialAmountException(decimal initialAmount)
            : base($"Initial amount \"{initialAmount}\" is invalid, must be at least 0.0") { }
    }
}
