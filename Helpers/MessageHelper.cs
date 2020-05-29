using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Helpers
{
    internal static class MessageHelper
    {
        public static async Task<DiscordMessage> SendErrorEmbed(CommandContext ctx, string message)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0xAA0000),
                Description = message,
                Title = "Error",
                ThumbnailUrl = FacesHelper.GetErrorFace()
            };
            return await ctx.RespondAsync(embed: embed);
        }

        public static async Task<DiscordMessage> SendWarningEmbed(CommandContext ctx, string message)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0xAAAA00),
                Description = message,
                Title = "Warning",
                ThumbnailUrl = FacesHelper.GetWarningFace()
            };
            return await ctx.RespondAsync(embed: embed);
        }

        public static async Task<DiscordMessage> SendSuccessEmbed(CommandContext ctx, string message)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x00AA00),
                Description = message,
                Title = "Success",
                ThumbnailUrl = FacesHelper.GetSuccessFace()
            };
            return await ctx.RespondAsync(embed: embed);
        }

        public static DiscordEmbedBuilder.EmbedFooter BuildFooter(string text, string iconurl = "")
        {
            var footer = new DiscordEmbedBuilder.EmbedFooter();
            footer.Text = text;
            footer.IconUrl = iconurl;
            return footer;
        }
    }
}
