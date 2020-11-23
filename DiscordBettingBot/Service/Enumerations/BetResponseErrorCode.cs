using System.ComponentModel;

namespace DiscordBettingBot.Service.Enumerations
{
    public enum BetResponseErrorCode
    {
        [Description("Bet made successfully")]
        Success = 1,
        [Description("Insufficent funds")]
        InsufficientFunds = 2,
        [Description("Better does not exist")]
        BetterDoesNotExist = 3,
        [Description("Match does not exist")]
        MatchDoesNotExist = 4,
        [Description("Invalid fund")]
        InvalidFund = 5,
    }
}
