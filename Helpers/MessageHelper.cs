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
                .WithTitle(":paperclip: Information")
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
                .WithTitle(":radioactive: Warning")
                .WithThumbnail(FacesHelper.GetWarningFace());

            return await ctx.RespondAsync(embed: embed);
        }

        public static DiscordEmbed SuccessEmbed(string message)
        {
            return new DiscordEmbedBuilder()
                .WithColor(0xff2e63)
                .WithDescription(message)
                .WithTitle(":white_check_mark: Success")
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
        static FacesConfig FacesData;

        static FacesHelper()
        {
            ReloadConfig();
        }
        public static void ReloadConfig()
        {
            var config = Config.LoadFromFile("config.json");

            PageUri = config.facesEndpoint;
            FacesData = config.facesConfig;
        }

        public static string GetIdleFace()
        {
            return PageUri + RandomEntry(FacesData.IdleFaces);
        }

        public static string GetErrorFace()
        {
            return PageUri + RandomEntry(FacesData.ErrorFaces);
        }

        public static string GetSuccessFace()
        {
            return PageUri + RandomEntry(FacesData.SuccessFaces);
        }

        public static string GetWarningFace()
        {
            return PageUri + RandomEntry(FacesData.WarningFaces);
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
        static string[] dudasAnswers;

        static QuotesHelper()
        {
            ReloadConfig();
        }
        public static void ReloadConfig()
        {
            var config = Config.LoadFromFile("config.json");

            statusList = config.statusMessages;
            dudasAnswers = config.dudaAnswers;
        }

        public static string[] GetStatusList() => statusList;

        public static string GetDudaAnswer()
        {
            return dudasAnswers[NumbersHelper.GetRandom(0, dudasAnswers.Length - 1)];
        }
    }
}
