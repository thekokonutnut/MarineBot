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
    [Description("Comandos de imágenes online.")]
    [ShortCommandsGroup]
    internal class ImageCommands : BaseCommandModule
    {
        private Config _config;
        private ImgurHelper _imgur;

        public ImageCommands(IServiceProvider serviceProvider)
        {
            _config = serviceProvider.GetService<Config>();
            _imgur = new ImgurHelper(_config.imgurClientID);
        }

        [GroupCommand(), Hidden()]
        public async Task MainCommand(CommandContext ctx)
        {
            var cmds = ctx.CommandsNext;
            var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), ctx.Command.QualifiedName);
            await cmds.ExecuteCommandAsync(context);
        }

        [Command("imgur"), Description("Obtiene una imágen al azar de Imgur dado una o más etiquetas.")]
        [Example("images imgur cat", "img imgur dog")]
        public async Task ImgurCommand(CommandContext ctx, [Description("Etiqueta(s) a buscar"), RemainingText()] string tag)
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

        [Command("safebooru"), Description("Obtiene una imágen de Safebooru al azar dado una o más etiquetas.")]
        [Example("images safebooru blue_eyes blush", "img safebooru touhou highres")]
        public async Task SafebooruCommand(CommandContext ctx, [Description("Etiqueta(s) a buscar"), RemainingText()] string tag)
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

        [Command("safebooru:tag"), Description("Busca etiquetas de safebooru.")]
        [Example("images safebooru:tag nezuko", "img safebooru:tag shingeki")]
        public async Task SafebooruTagCommand(CommandContext ctx, [Description("Etiqueta a buscar"), RemainingText()] string tag)
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

        [Command("gelbooru"), Description("Obtiene una imágen de Gelbooru al azar dado una o más etiquetas.")]
        [Example("images gelbooru blue_eyes blush", "img gelbooru touhou highres")]
        [RequireNsfw()]
        public async Task GelbooruCommand(CommandContext ctx, [Description("Etiqueta(s) a buscar"), RemainingText()] string tag)
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

        [Command("gelbooru:tag"), Description("Busca etiquetas de gelbooru.")]
        [Example("images gelbooru:tag nezuko", "img gelbooru:tag shingeki")]
        public async Task GelbooruTagCommand(CommandContext ctx, [Description("Etiqueta a buscar"), RemainingText()] string tag)
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

        [Command("e621"), Description("Obtiene una imágen de E621 al azar dado una o más etiquetas.")]
        [Example("images e621 rating:safe human_only", "img e621 -anthro -furry")]
        [RequireNsfw()]
        public async Task E621Command(CommandContext ctx, [Description("Etiqueta(s) a buscar"), RemainingText()] string tag)
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

        [Command("emoji"), Description("Obtiene una emoji al azar. ¿De donde? Nose.")]
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

        [Command("discord"), Description("Obtiene una imágen de Discord al azar. ¿De donde? Nose.")]
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
    }
}
