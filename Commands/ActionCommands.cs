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
    [Description("Comandos de acciones.")]
    internal class ActionCommands : BaseCommandModule
    {
        public ActionCommands(IServiceProvider serviceProvider)
        {

        }

        [GroupCommand(), Hidden()]
        public async Task MainCommand(CommandContext ctx)
        {
            var cmds = ctx.CommandsNext;
            var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), ctx.Command.QualifiedName);
            await cmds.ExecuteCommandAsync(context);
        }

        [Command("slap"), Description("Por pete.")]
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
                await ctx.RespondWithFileAsync("slap.gif", generatedGif);

                generatedGif.Dispose();
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message, false);
                throw;
            }
            finally
            {
                avatar1.Dispose();
                avatar2.Dispose();
            }
        }
    }
}
