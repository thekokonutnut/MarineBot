using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using MarineBot.Threads;
using MarineBot.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MarineBot.Entities;
using MarineBot.Helpers;
using MarineBot.Controller;
using System.Linq;
using MarineBot.Attributes;

namespace MarineBot.Commands
{
    [Group("Polls"), Aliases("p")]
    [Description("Comandos de utilidad variada.")]
    [RequireGuild]
    internal class PollCommands : BaseCommandModule
    {
        private PollTable _pollTable;

        public PollCommands(IServiceProvider serviceProvider)
        {
            var controller = (DatabaseController)serviceProvider.GetService(typeof(DatabaseController));

            _pollTable = controller.GetTable<PollTable>();
        }

        [GroupCommand(), Hidden()]
        public async Task MainCommand(CommandContext ctx)
        {
            var cmds = ctx.CommandsNext;
            var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), ctx.Command.QualifiedName);
            await cmds.ExecuteCommandAsync(context);
        }

        [Command("create"), Description("Crea una encuesta.")]
        [Example("polls create \"L4D2 Night\" 3600", "p create \"Is snes gay\" 60 Yes Yes Yes", "p create \"Best game\" 120 \"Minecraft 2\" \"Terraria 4\" \"Pete Adventures\"")]
        public async Task CreatePollCommand(CommandContext ctx, [Description("Titulo de la encuesta.")] string title, 
                          [Description("Tiempo en segundos.")] uint time = 10, [Description("Opciones."), RemainingText()] params string[] options)
        {
            if (options == null) throw new ArgumentException();

            if (title.Length == 0)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Necesitas especificar un titulo.");
                return;
            }
            if (time < 10)
            {
                await MessageHelper.SendErrorEmbed(ctx, "El tiempo debe ser mínimo 10 segundos.");
                return;
            }
            if (options.Length < 2 || options.Length > 5)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Necesitas especificar mínimo 2 opciones y máximo 5.");
                return;
            }

            var pollMsg = await MessageHelper.SendInfoEmbed(ctx, "Creando encuesta...");
            var poll = new Poll(pollMsg.Id, pollMsg.Channel.GuildId, pollMsg.ChannelId, title, time, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), options.ToList());
            _pollTable.CreatePoll(poll);
        }
    }
}
