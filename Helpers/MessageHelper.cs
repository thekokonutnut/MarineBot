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
        public static DiscordEmbed InfoEmbed(string message)
        {
            return new DiscordEmbedBuilder()
                .WithColor(0xeaeaea)
                .WithDescription(message)
                .WithTitle(":paperclip: Información")
                .WithThumbnail(FacesHelper.GetIdleFace());
        }

        public static async Task<DiscordMessage> SendInfoEmbed(CommandContext ctx, string message)
        {
            var embed = InfoEmbed(message);
            return await ctx.RespondAsync(embed: embed);
        }

        public static DiscordEmbed ErrorEmbed(string message)
        {
            return new DiscordEmbedBuilder()
                .WithColor(0x252a34)
                .WithDescription(message)
                .WithTitle(":name_badge: Error")
                .WithThumbnail(FacesHelper.GetErrorFace());
        }
        public static async Task<DiscordMessage> SendErrorEmbed(CommandContext ctx, string message, bool delete = false)
        {
            var embed = ErrorEmbed(message);

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
                .WithColor(0x08d9d6)
                .WithDescription(message)
                .WithTitle(":radioactive: Advertencia")
                .WithThumbnail(FacesHelper.GetWarningFace());

            return await ctx.RespondAsync(embed: embed);
        }

        public static DiscordEmbed SuccessEmbed(string message)
        {
            return new DiscordEmbedBuilder()
                .WithColor(0xff2e63)
                .WithDescription(message)
                .WithTitle(":white_check_mark: Exitoso")
                .WithThumbnail(FacesHelper.GetSuccessFace());
        }
        public static async Task<DiscordMessage> SendSuccessEmbed(CommandContext ctx, string message)
        {
            var embed = SuccessEmbed(message);

            return await ctx.RespondAsync(embed: embed);
        }
    }

    internal static class FacesHelper
    {
        static string PageUri = "";

        static FacesHelper()
        {
            var config = Config.LoadFromFile("config.json");

            PageUri = config.facesEndpoint;
        }

        public static string GetIdleFace()
        {
            string[] faces = { "Confused_0.png", "Confused_1.png", "Confused_2.png", "Cute_0.png", "Eugh_1.png",
                                "Happy_4.png", "Smug_0.png", "Relax_0.png", "Smug_3.png", "Smug_2.png", "Smug_6.png",
                                "Smug_0.png", "Surprise_1.png", "Happy_1.png", "Ok_1.png", "Happy_5.png", "Confused_5.png",
                                "Muda_0.png", "Angry_2.png", "Ok_0.png", "Blush_0.png"};
            return PageUri + RandomEntry(faces);
        }

        public static string GetErrorFace()
        {
            string[] faces = { "Angry_0.png", "Confused_2.png", "Disgusting_0.png", "Disgusting_1.png", "Sad_0.png",
                                "Sad_1.png", "Sad_2.png", "Sad_3.png", "Scary_0.png", "Cute_0.png", "Confused_3.png",
                                "Confused_5.png", "Muda_0.png", "Angry_2.png", "Confused_4.png", "Angry_1.png"};
            return PageUri + RandomEntry(faces);
        }

        public static string GetSuccessFace()
        {
            string[] faces = { "Happy_0.png", "Happy_1.png", "Happy_2.png", "Happy_3.png", "Happy_4.png", "Relax_0.png",
                                "Smug_0.png", "Smug_1.png", "Smug_2.png", "Smug_4.png", "Smug_6.png", "Smug_7.png", "Ok_1.png",
                                "Happy_5.png", "Happy_6.png", "Ok_0.png", "Blush_0.png" };
            return PageUri + RandomEntry(faces);
        }

        public static string GetWarningFace()
        {
            string[] faces = { "Angry_0.png", "Confused_0.png", "Eugh_0.png", "Scary_0.png", "Surprise_0.png",
                                "Surprise_1.png", "Smug_5.png", "Smug_3.png", "Smug_2.png", "Cute_0.png",
                                "Confused_3.png", "Confused_5.png", "Muda_0.png", "Angry_2.png", "Confused_4.png"};
            return PageUri + RandomEntry(faces);
        }

        public static string GetFace(string face)
        {
            return PageUri + face;
        }

        static string RandomEntry(string[] arr)
        {
            Random random = new Random();
            return arr[random.Next(0, arr.Length)];
        }
    }

    internal static class QuotesHelper
    {
        static string[] statusList;

        static QuotesHelper()
        {
            var config = Config.LoadFromFile("config.json");

            statusList = config.statusMessages;
        }

        public static string GetRandomStatus()
        {
            return statusList[NumbersHelper.GetRandom(0, statusList.Length - 1)];
        }
    }
}
