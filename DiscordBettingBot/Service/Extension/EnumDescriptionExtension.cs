using System;
using System.ComponentModel;

namespace DiscordBettingBot.Common.Service.Extension
{
    public static class MyEnumExtensions
    {
        public static string GetDescription<T>(this T enumVal) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            var attributes = (DescriptionAttribute[])enumVal
                .GetType()
                .GetField(enumVal.ToString() ?? string.Empty)
                ?.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes != null && attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}
