using MarineBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using CodingSeb.ExpressionEvaluator;
using DSharpPlus.Entities;
using MarineBot.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace MarineBot.Commands
{
    [Group("Images"), Aliases("img")]
    [Description("Online image commands.")]
    [ShortCommandsGroup]
    internal class ImageCommands : BaseCommandModule
    {
        private Config _config;
        private ImgurHelper _imgur;
        private SauceHelper _sauce;

        public ImageCommands(IServiceProvider serviceProvider)
        {
            _config = serviceProvider.GetService<Config>();
            _imgur = new ImgurHelper(_config.imgurClientID);
            _sauce = new SauceHelper(_config.saucenaoApiKey);
        }

        [GroupCommand(), Hidden()]
        public async Task MainCommand(CommandContext ctx)
        {
            var cmds = ctx.CommandsNext;
            var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), ctx.Command.QualifiedName);
            await cmds.ExecuteCommandAsync(context);
        }

        [Command("imgur"), Description("Gets a random image from Imgur given one or more tags.")]
        [Example("images imgur cat", "img imgur dog")]
        public async Task ImgurCommand(CommandContext ctx, [Description("Tag(s) to search for"), RemainingText()] string tag)
        {
            if (tag == null) throw new ArgumentException();
            try
            {
                try
                {
                    var ranImg = await _imgur.GetRandomImage(tag);
                    var embed = new DiscordEmbedBuilder()
                        .WithColor(0x3d9dd1)
                        .WithTitle(ranImg.Title)
                        .WithUrl(ranImg.Link)
                        .WithAuthor(ranImg.Uploader)
                        .WithFooter(ranImg.Link)
                        .WithImageUrl(ranImg.ImageLink);

                    await ctx.RespondAsync(embed: embed);
                }
                catch (Exception e)
                {
                    await MessageHelper.SendWarningEmbed(ctx, e.Message);
                }
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("safebooru"), Description("Gets a random image from Safebooru given one or more tags.")]
        [Example("images safebooru blue_eyes blush", "img safebooru touhou highres")]
        public async Task SafebooruCommand(CommandContext ctx, [Description("Tag(s) to search for"), RemainingText()] string tag)
        {
            if (tag == null) throw new ArgumentException();
            try
            {
                try
                {
                    var ranImg = await SafebooruHelper.GetRandomImage(tag);
                    var embed = new DiscordEmbedBuilder()
                        .WithColor(0x3d9dd1)
                        .WithFooter($"https://safebooru.org/index.php?page=post&s=view&id={ranImg.Id}")
                        .WithImageUrl(ranImg.File_url);

                    await ctx.RespondAsync(embed: embed);
                }
                catch (Exception e)
                {
                    await MessageHelper.SendWarningEmbed(ctx, e.Message);
                }
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("safebooru:tag"), Description("Search for Safebooru tags.")]
        [Example("images safebooru:tag nezuko", "img safebooru:tag shingeki")]
        public async Task SafebooruTagCommand(CommandContext ctx, [Description("Tag to search for"), RemainingText()] string tag)
        {
            if (tag == null) throw new ArgumentException();
            try
            {
                try
                {
                    var ranImg = await SafebooruHelper.SearchForTag(tag);

                    var sb = new StringBuilder();

                    for (int i = 0; i < ranImg.Count; i++)
                    {
                        sb.Append($"{i + 1}. {Formatter.InlineCode(ranImg[i].Name)} ({ranImg[i].Count})\n");
                    }

                    await MessageHelper.SendSuccessEmbed(ctx, sb.ToString());
                }
                catch (Exception e)
                {
                    await MessageHelper.SendWarningEmbed(ctx, e.Message);
                }
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("gelbooru"), Description("Gets a random image from Gelbooru given one or more tags.")]
        [Example("images gelbooru blue_eyes blush", "img gelbooru touhou highres")]
        [RequireNsfw()]
        public async Task GelbooruCommand(CommandContext ctx, [Description("Tag(s) to search for"), RemainingText()] string tag)
        {
            if (tag == null) throw new ArgumentException();
            try
            {
                try
                {
                    var ranImg = await GelbooruHelper.GetRandomImage(tag);
                    var embed = new DiscordEmbedBuilder()
                        .WithColor(0x3d9dd1)
                        .WithFooter($"https://gelbooru.com/index.php?page=post&s=view&id={ranImg.Id}")
                        .WithImageUrl(ranImg.File_url);

                    await ctx.RespondAsync(embed: embed);
                }
                catch (Exception e)
                {
                    await MessageHelper.SendWarningEmbed(ctx, e.Message);
                }
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("gelbooru:tag"), Description("Search for Safebooru tags.")]
        [Example("images gelbooru:tag nezuko", "img gelbooru:tag shingeki")]
        public async Task GelbooruTagCommand(CommandContext ctx, [Description("Tag to search for"), RemainingText()] string tag)
        {
            if (tag == null) throw new ArgumentException();
            try
            {
                try
                {
                    var ranImg = await GelbooruHelper.SearchForTag(tag);

                    var sb = new StringBuilder();

                    for (int i = 0; i < ranImg.Count; i++)
                    {
                        sb.Append($"{i+1}. {Formatter.InlineCode(ranImg[i].Name)} ({ranImg[i].Count})\n");
                    }

                    await MessageHelper.SendSuccessEmbed(ctx, sb.ToString());
                }
                catch (Exception e)
                {
                    await MessageHelper.SendWarningEmbed(ctx, e.Message);
                }
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("e621"), Description("Gets a random image from E621 given one or more tags.")]
        [Example("images e621 rating:safe human_only", "img e621 -anthro -furry")]
        [RequireNsfw()]
        [Hidden]
        public async Task E621Command(CommandContext ctx, [Description("Tag(s) to search for"), RemainingText()] string tag)
        {
            if (tag == null) throw new ArgumentException();
            try
            {
                try
                {
                    var ranImg = await E621Helper.GetRandomImage(tag);
                    var embed = new DiscordEmbedBuilder()
                        .WithColor(0x3d9dd1)
                        .WithFooter($"https://e621.net/posts/{ranImg.Id}")
                        .WithImageUrl(ranImg.ImageUrl);

                    await ctx.RespondAsync(embed: embed);
                }
                catch (Exception e)
                {
                    await MessageHelper.SendWarningEmbed(ctx, e.Message);
                }
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("emoji"), Description("You get a random emoji. From where? I don't know.")]
        [Example("images emoji")]
        public async Task RandomEmojiCommand(CommandContext ctx)
        {
            try
            {
                var ranImg = await EmojiHelper.GetRandomOnlineEmoji();

                await ctx.RespondAsync(ranImg);
            }
            catch (Exception e)
            {
                await MessageHelper.SendWarningEmbed(ctx, e.Message);
            }
        }

        [Command("discord"), Description("Get a random image of Discord. From where? I don't know.")]
        [Example("images discord")]
        public async Task RandomImageCommand(CommandContext ctx)
        {
            try
            {
                var ranImg = await EmojiHelper.GetRandomOnlineImage();

                await ctx.RespondAsync(ranImg);
            }
            catch (Exception e)
            {
                await MessageHelper.SendWarningEmbed(ctx, e.Message);
            }
        }

        [Command("sauce"), Description("Find sauce for a image.")]
        [Example("sauce https://catimages.com/cat1.jpg", "sauce (attach image)")]
        public async Task SauceCommand(CommandContext ctx, [Description("Image URL"), RemainingText()] string imgurl = "")
        {
            if (string.IsNullOrEmpty(imgurl))
            {
                if (ctx.Message.Attachments.Count > 0)
                {
                    imgurl = ctx.Message.Attachments[0].Url;
                }
                else if (ctx.Message.MessageType == MessageType.Reply)
                {
                    if (ctx.Message.ReferencedMessage.Attachments.Count > 0)
                    {
                        imgurl = ctx.Message.ReferencedMessage.Attachments[0].Url;
                    }
                }

                if (string.IsNullOrEmpty(imgurl))
                    throw new ArgumentException();
            }
            try
            {
                var sourceData = await _sauce.SaucenaoSearch(imgurl);

                var embed = new DiscordEmbedBuilder()
                    .WithColor(0xff00b3)
                    .WithTitle("got 'em")
                    .WithFooter($"Showing result 1 of {sourceData.Count} - Powered by SauceNao")
                    .WithImageUrl(sourceData[0].Thumbnail);

                var sb = new StringBuilder();
                sb.AppendLine($"Got sauce with {sourceData[0].Similarity}% similarity.");
                if (sourceData[0].ExternalUrls.Count != 0)
                    sb.AppendLine($"Source: [{sourceData[0].IndexName}]({sourceData[0].ExternalUrls[0]})");

                embed.WithDescription(sb.ToString());
                sb.Clear();

                foreach (var extra in sourceData[0].ExtraInfo)
                {
                    sb.AppendLine($"{extra.Key}: {extra.Value}");
                }
                embed.AddField("Extra info", sb.ToString());
                
                await ctx.RespondAsync(embed: embed);
            }
            catch (Exception e)
            {
                await MessageHelper.SendWarningEmbed(ctx, e.Message);
            }
        }
    }
}
