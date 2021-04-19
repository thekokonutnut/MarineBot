using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MarineBot.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Attributes
{
    class RequireBotAdministratorAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return Task.FromResult(AuthHelper.BotAdministrator(ctx.User));
        }
    }
}
