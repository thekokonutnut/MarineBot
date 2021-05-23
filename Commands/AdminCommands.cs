using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Exceptions;
using MarineBot.Attributes;
using MarineBot.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MarineBot.Commands
{
    [Group("admin")]
    [Description("Comandos de administración.")]
    [Hidden, RequireBotAdministrator]
    internal class AdminCommands : BaseCommandModule
    {
        private CancellationTokenSource _cts;

        public AdminCommands(IServiceProvider serviceProvider)
        {
            _cts = (CancellationTokenSource)serviceProvider.GetService(typeof(CancellationTokenSource));
        }

        [GroupCommand(), Hidden()]
        public async Task MainCommand(CommandContext ctx)
        {
            var cmds = ctx.CommandsNext;
            var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), ctx.Command.QualifiedName);
            await cmds.ExecuteCommandAsync(context);
        }

        [Command("shutdown"), Description("Apaga el bot.")]
        public async Task ShutdownCommand(CommandContext ctx)
        {
            try
            {
                await MessageHelper.SendInfoEmbed(ctx, "Apagando el bot...");
                _cts.Cancel();
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }
    }
}
