using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBettingBot.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequireRoleAttribute : RequireContextAttribute
    {
        private readonly ulong _requiredRole;


        public RequireRoleAttribute(ulong requiredRole) : base(ContextType.Guild)
        {
            _requiredRole = requiredRole;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(
            ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var baseResult = await base.CheckPermissionsAsync(context, command, services);
            if (!baseResult.IsSuccess)
                return baseResult;

            return (((IGuildUser)context.User).RoleIds.Contains(_requiredRole))
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("User does not have the required role.");
        }
    }
}
