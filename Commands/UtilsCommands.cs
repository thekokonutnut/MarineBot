using MarineBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;
using DSharpPlus;
using CodingSeb.ExpressionEvaluator;
using MarineBot.Attributes;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Entities;
using MarineBot.Controller;
using System.Linq;
using System.Text;
using MarineBot.Entities;

namespace MarineBot.Commands
{
    [Group("Utils"), Aliases("u")]
    [Description("Commands of varied utility.")]
    [ShortCommandsGroup]
    [RequireGuild]
    internal class UtilsCommands : BaseCommandModule
    {
        private Config _config;
        private YoutubeHelper _youtube;
        private SmugresponsesController _smugresponseController;

        public UtilsCommands(IServiceProvider serviceProvider)
        {
            _config = serviceProvider.GetService<Config>();
            _youtube = new YoutubeHelper(_config.ytAPIEndpoint);
            _smugresponseController = serviceProvider.GetService<SmugresponsesController>();
        }

        [GroupCommand(), Hidden()]
        public async Task MainCommand(CommandContext ctx)
        {
            var cmds = ctx.CommandsNext;
            var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), ctx.Command.QualifiedName);
            await cmds.ExecuteCommandAsync(context);
        }

        [Command("roll"), Description("Roll a random number.")]
        [Example("utils roll 30", "u roll 100 60")]
        public async Task RollCommand(CommandContext ctx, [Description("Maximum number.")] int max, [Description("Minimum number.")] int min = 0)
        {
            try
            {
                int rand = NumbersHelper.GetRandom(min, max);
                await MessageHelper.SendSuccessEmbed(ctx, $"`Result:` {rand}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("choose"), Description("Choose one of the options provided.")]
        [Example("utils choose Yes No", "u choose \"Puede ser\" \"No creo\" \"Xd!\"")]
        public async Task ChooseCommand(CommandContext ctx, [Description("Options."), RemainingText()] params string[] options)
        {
            if (options == null) throw new ArgumentException();
            try
            {
                if (options.Length < 2)
                {
                    await MessageHelper.SendWarningEmbed(ctx, "I need at least two options.");
                    return;
                }

                if (NumbersHelper.GetRandom(0, 100) > 95)
                {
                    await MessageHelper.SendWarningEmbed(ctx, "`I can't decide...`");
                    return;
                }

                int chosen = NumbersHelper.GetRandom(0, options.Length - 1);
                await MessageHelper.SendSuccessEmbed(ctx, $"`Result:` {options[chosen]}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("question"), Description("Clear your doubts.")]
        [Example("utils question Is snessy gay?", "u question Are you sure?")]
        public async Task DudaCommand(CommandContext ctx, [Description("Question"), RemainingText()] string question)
        {
            if (question == null) throw new ArgumentException();
            try
            {
                await MessageHelper.SendSuccessEmbed(ctx, $"`{question}:` {QuotesHelper.GetDudaAnswer()}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("purge"), Description("Deletes the specified number of messages.")]
        [Example("utils purge 10")]
        [RequireUserPermissions(Permissions.ManageMessages), RequireBotPermissions(Permissions.ManageMessages)]
        public async Task PurgeCommand(CommandContext ctx, [Description("Number of messages")] uint amount)
        {
            try
            {
                var messages = await ctx.Channel.GetMessagesAsync((int)amount + 1);

                await ctx.Channel.DeleteMessagesAsync(messages);
                var m = MessageHelper.SendSuccessEmbed(ctx, $"Messages deleted successfully.");
                await Task.Delay(2000);
                await m.Result.DeleteAsync();
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("youtube"), Description("Downloads mp3 or mp4 from youtube."), Aliases("yt")]
        [Example("utils youtube https://www.youtube.com/watch?v=IfnoD0tCzD8")]
        public async Task YoutubeCommand(CommandContext ctx, [Description("Youtube video link")] string video, [Description("Convertion format (available: mp3, mp4)")] string format = "mp3")
        {
            try
            {
                // TODO: validate url and format


                await MessageHelper.SendInfoEmbed(ctx, $"Processing video URL: {video}");

                var procVid = await _youtube.ProcessVideoURL(video, format);

                var embed = new DiscordEmbedBuilder()
                    .WithColor(0x3d9dd1)
                    .WithTitle("Your video has been converted!")
                    .WithFooter("powered by kytd !!!!")
                    .WithThumbnail(FacesHelper.GetSuccessFace());
                //.WithImageUrl(ranImg.ImageLink); // get video thumb

                embed.WithDescription($"**Title:** {procVid.Title}\n**Duration:** {procVid.Duration} seconds\n\n**Format:** {format.ToUpper()}\n**Filesize:** {procVid.Filesize / (1024 * 1024)} MB\n\n[Download file]({procVid.DownloadLink})\n");

                await ctx.RespondAsync(embed: embed);
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("smugresponsechan"), Description("Toggles the smugresponses for this channel.")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task SmugresponsesChanCommand(CommandContext ctx)
        {
            try
            {
                _smugresponseController.ToggleEnabledChannelResponses(ctx.Channel.Id);

                if (_smugresponseController.IsEnabledChannelResponses(ctx.Channel.Id))
                    await MessageHelper.SendSuccessEmbed(ctx, "Smug responses enabled for this channel!");
                else
                    await MessageHelper.SendSuccessEmbed(ctx, "Smug responses disabled for this channel!");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("smugresponseguild"), Description("Toggles the smugresponses for the entire guild.")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task SmugresponsesGuildCommand(CommandContext ctx)
        {
            try
            {
                _smugresponseController.ToggleEnabledGuildResponses(ctx.Guild.Id);

                if (_smugresponseController.IsEnabledGuildResponses(ctx.Guild.Id))
                    await MessageHelper.SendSuccessEmbed(ctx, "Smug responses enabled for this guild!");
                else
                    await MessageHelper.SendSuccessEmbed(ctx, "Smug responses disabled for this guild!");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("anime"), Description("Search for anime details."), Aliases("a")]
        [Example("utils anime One Piece", "utils a 5667")]
        public async Task AnimeSearchCommand(CommandContext ctx, [Description("Search query or MAL Id"), RemainingText()] string query)
        {
            try
            {
                MALAnimeEntry anime;
                if (query == null)
                {
                    anime = await MALHelper.GetRandomAnimeEntry();
                }
                else if (int.TryParse(query, out int id))
                {
                    anime = await MALHelper.GetAnimeEntryById(id);
                }
                else
                {
                    var animeList = await MALHelper.GetAnimeEntriesAsync(query);
                    anime = animeList.First();
                }

                if (anime != null)
                {
                    var embed = new DiscordEmbedBuilder()
                        .WithColor(0x1b62b5);
                    embed.Title = anime.Title;
                    embed.Url = anime.Url;
                    embed.Description = anime.Title_Japanese;

                    if (anime.Year != null)
                        embed.AddField("Year", anime.Year.ToString(), true);
                    else
                        embed.AddField("Year", anime.Aired.From?.Year.ToString() ?? "Unknown", true);

                    embed.AddField("Episodes", anime.Episodes?.ToString() ?? "Unknown", true);
                    embed.AddField("Status", anime.Status ?? "Unknown", true);
                    embed.AddField("Type", anime.Type ?? "Unknown", true);
                    embed.AddField("Rating", anime.Rating ?? "Unknown", true);

                    if (anime.Genres != null && anime.Genres.Count > 0)
                    {
                        string genres = string.Join(", ", anime.Genres.Select(g => g.Name));
                        embed.AddField("Genres", genres, true);
                    }

                    if (anime.Producers != null && anime.Producers.Count > 0)
                    {
                        string producers = string.Join(", ", anime.Producers.Select(p => p.Name));
                        embed.AddField("Producers", producers, true);
                    }

                    if (anime.Studios != null && anime.Studios.Count > 0)
                    {
                        string studios = string.Join(", ", anime.Studios.Select(s => s.Name));
                        embed.AddField("Studios", studios, true);
                    }

                    string synopsis = anime.Synopsis.Length > 280 ? anime.Synopsis.Substring(0, 280) + "..." : anime.Synopsis;
                    embed.AddField("Synopsis", synopsis, false);

                    embed.WithFooter($"Scored {anime.Score ?? 0} by {anime.Scored_By ?? 0} users /// Powered by Jikan");

                    if (anime.Images != null && !String.IsNullOrWhiteSpace(anime.Images.Jpg.Image_Url))
                        embed.WithThumbnail(anime.Images.Jpg.Image_Url);

                    await ctx.RespondAsync(embed: embed);
                }
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("anime:list"), Description("Get a listing of animes matching a query."), Aliases("a")]
        [Example("utils anime One Piece")]
        public async Task AnimeListingCommand(CommandContext ctx, [Description("Search query"), RemainingText()] string query)
        {
            if (query == null) throw new ArgumentException();

            try
            {
                var animeList = await MALHelper.GetAnimeEntriesAsync(query, 5);

                var embed = new DiscordEmbedBuilder()
                        .WithColor(0x1b62b5)
                        .WithTitle("Search results");

                var sb = new StringBuilder();

                foreach (var item in animeList)
                {
                    sb.AppendLine($"{item.Mal_Id} - {item.Title}");
                }

                embed.AddField("MAL Id - Anime title", sb.ToString());

                await ctx.RespondAsync(embed: embed);
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }
    }
}