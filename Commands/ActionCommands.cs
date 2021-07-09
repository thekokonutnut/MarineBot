using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MarineBot.Attributes;
using MarineBot.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarineBot.Commands
{
    [Group("Actions"), Aliases("act")]
    [Description("Action commands.")]
    [RequireGuild]
    internal class ActionCommands : BaseCommandModule
    {
        [GroupCommand(), Hidden()]
        public async Task MainCommand(CommandContext ctx)
        {
            var cmds = ctx.CommandsNext;
            var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), ctx.Command.QualifiedName);
            await cmds.ExecuteCommandAsync(context);
        }

        [Command("slap"), Description("Fag.")]
        [Example("slap @Snessy")]
        public async Task SlapCommand(CommandContext ctx, [Description("Who")] DiscordMember member)
        {
            var avatar1Url = Regex.Replace(ctx.User.AvatarUrl, @"size\=.+", "size=256");
            var avatar2Url = Regex.Replace(member.AvatarUrl, @"size\=.+", "size=256");

            var avatar1 = ImageHelper.LoadImage(avatar1Url);
            var avatar2 = ImageHelper.LoadImage(avatar2Url);

            try
            {
                var generatedGif = ImageHelper.GenerateGif("gifs/slap/", avatar1, avatar2);
                var builder = new DiscordMessageBuilder().WithFile("slap.gif", generatedGif);
                await ctx.RespondAsync(builder);

                generatedGif.Dispose();
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
            finally
            {
                avatar1.Dispose();
                avatar2.Dispose();
            }
        }
    }
}
