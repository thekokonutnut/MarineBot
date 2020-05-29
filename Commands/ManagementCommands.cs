using MarineBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;

namespace MarineBot.Commands
{
    [Group("Management"),Aliases("m")]
    [Description("Comandos de administración.")]
    [RequireUserPermissions(Permissions.Administrator)]
    [Hidden]
    internal class ManagementCommands : BaseCommandModule
    {
        readonly CancellationTokenSource _cts;
        public ManagementCommands(IServiceProvider serviceProvider)
        {
            _cts = (CancellationTokenSource)serviceProvider.GetService(typeof(CancellationTokenSource));
        }

        [Command("shutdown")]
        [Description("Apaga el bot.")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ShutdownCommand(CommandContext ctx)
        {
            await MessageHelper.SendSuccessEmbed(ctx, "Apagando el bot.");
            _cts.Cancel();
        }
    }
}
