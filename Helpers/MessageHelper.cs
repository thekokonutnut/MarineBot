using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MarineBot.Helpers
{
    internal static class MessageHelper
    {
        public static async Task<DiscordMessage> SendInfoEmbed(CommandContext ctx, string message)
        {
            var embed = new DiscordEmbedBuilder()
                .WithColor(0x3d9dd1)
                .WithDescription(message)
                .WithTitle(":paperclip::paperclip::paperclip: Info")
                .WithThumbnail(FacesHelper.GetIdleFace());

            return await ctx.RespondAsync(embed: embed);
        }

        public static async Task<DiscordMessage> SendErrorEmbed(CommandContext ctx, string message, bool delete = true)
        {
            var embed = new DiscordEmbedBuilder()
                .WithColor(0xAA0000)
                .WithDescription(message)
                .WithTitle(":name_badge::name_badge::name_badge: Error")
                .WithThumbnail(FacesHelper.GetErrorFace());

            var msg = await ctx.RespondAsync(embed: embed);
            if (delete)
                new Thread(async () => {
                    await Task.Delay(5000);
                    await msg.DeleteAsync();
                }).Start();
            return msg;
        }

        public static async Task<DiscordMessage> SendWarningEmbed(CommandContext ctx, string message)
        {
            var embed = new DiscordEmbedBuilder()
                .WithColor(0xAAAA00)
                .WithDescription(message)
                .WithTitle(":radioactive::radioactive::radioactive: Warning")
                .WithThumbnail(FacesHelper.GetWarningFace());

            return await ctx.RespondAsync(embed: embed);
        }

        public static async Task<DiscordMessage> SendSuccessEmbed(CommandContext ctx, string message)
        {
            var embed = new DiscordEmbedBuilder()
                .WithColor(0x00AA00)
                .WithDescription(message)
                .WithTitle(":white_check_mark::white_check_mark::white_check_mark: Success")
                .WithThumbnail(FacesHelper.GetSuccessFace());

            return await ctx.RespondAsync(embed: embed);
        }
    }
}
