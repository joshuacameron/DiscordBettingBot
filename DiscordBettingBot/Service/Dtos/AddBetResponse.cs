using DiscordBettingBot.Service.Enumerations;
using DiscordBettingBot.Service.Extension;

namespace DiscordBettingBot.Service.Dtos
{
    public class AddBetResponse
    {
        public bool IsSuccessful { get; set; }
        public BetResponseErrorCode ErrorCode { get; set; }
        public string ErrorMessage => ErrorCode.GetDescription();
    }
}
