using System;

namespace DiscordBettingBot.Common.Service.Exceptions
{
    public class InsufficientFundsException : Exception
    {
        public InsufficientFundsException(decimal betAmount, decimal balance)
            : base($"Bet was \"{betAmount}\" where better only has {balance} to bet with") { }
    }
}
