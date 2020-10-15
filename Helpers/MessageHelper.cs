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
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x3d9dd1),
                Description = message,
                Title = ":paperclip: Info",
                Thumbnail = BuildThumb(FacesHelper.GetErrorFace())
            };
            return await ctx.RespondAsync(embed: embed);
        }

        public static async Task<DiscordMessage> SendErrorEmbed(CommandContext ctx, string message, bool delete = true)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0xAA0000),
                Description = message,
                Title = ":name_badge: Error",
                Thumbnail = BuildThumb(FacesHelper.GetErrorFace())
            };
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
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0xAAAA00),
                Description = message,
                Title = ":radioactive: Warning",
                Thumbnail = BuildThumb(FacesHelper.GetSuccessFace())
            };
            return await ctx.RespondAsync(embed: embed);
        }

        public static async Task<DiscordMessage> SendSuccessEmbed(CommandContext ctx, string message)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x00AA00),
                Description = message,
                Title = ":ballot_box_with_check: Success",
                Thumbnail = BuildThumb(FacesHelper.GetSuccessFace())
            };
            return await ctx.RespondAsync(embed: embed);
        }

        public static DiscordEmbedBuilder.EmbedFooter BuildFooter(string text, string iconurl = "")
        {
            var footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = text,
                IconUrl = iconurl
            };
            return footer;
        }

        public static DiscordEmbedBuilder.EmbedThumbnail BuildThumb(string uri)
        {
            var thumb = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = FacesHelper.GetIdleFace()
            };
            return thumb;
        }
    }
}
