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
using Microsoft.Extensions.DependencyInjection;

namespace MarineBot.Commands
{
    [Group("Polls"), Aliases("p", "poll")]
    [Description("Poll commands.")]
    [RequireGuild]
    internal class PollCommands : BaseCommandModule
    {
        private PollTable _pollTable;

        public PollCommands(IServiceProvider serviceProvider)
        {
            var controller = serviceProvider.GetService<DatabaseController>();

            _pollTable = controller.GetTable<PollTable>();
        }

        [GroupCommand(), Hidden()]
        public async Task MainCommand(CommandContext ctx)
        {
            var cmds = ctx.CommandsNext;
            var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), ctx.Command.QualifiedName);
            await cmds.ExecuteCommandAsync(context);
        }

        [Command("create"), Description("Create a poll.")]
        [Example("polls create \"L4D2 Night\" 3600", "p create \"Is snes gay\" 60 Yes Yes Yes", "p create \"Best game\" 120 \"Minecraft 2\" \"Terraria 4\" \"Pete Adventures\"")]
        public async Task CreatePollCommand(CommandContext ctx, [Description("Poll title.")] string title, 
                          [Description("Time in seconds.")] uint time = 10, [Description("Options."), RemainingText()] params string[] options)
        {
            if (options == null) throw new ArgumentException();

            if (title.Length == 0)
            {
                await MessageHelper.SendErrorEmbed(ctx, "You need to specify a title.");
                return;
            }
            if (time < 10)
            {
                await MessageHelper.SendErrorEmbed(ctx, "The time must be at least 10 seconds.");
                return;
            }
            if (options.Length < 2 || options.Length > 5)
            {
                await MessageHelper.SendErrorEmbed(ctx, "You need to specify a minimum of 2 options and a maximum of 5.");
                return;
            }

            var pollMsg = await MessageHelper.SendInfoEmbed(ctx, "Creating poll...");
            var poll = new Poll(pollMsg.Id, (ulong)pollMsg.Channel.GuildId, pollMsg.ChannelId, title, time, DateTimeOffset.UtcNow.ToUnixTimeSeconds(), options.ToList());
            _pollTable.CreatePoll(poll);
        }
    }
}
