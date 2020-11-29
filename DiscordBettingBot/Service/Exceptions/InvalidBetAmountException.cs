using System;

namespace DiscordBettingBot.Common.Service.Exceptions
{
    public class InvalidBetAmountException : Exception
    {
        public InvalidBetAmountException(decimal betAmount)
            : base($"Bet \"{betAmount}\" must be at maximum 2 decimal places and at least 0.01") { }
    }
}
