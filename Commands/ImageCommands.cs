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

namespace MarineBot.Commands
{
    [Group("Images"), Aliases("img")]
    [Description("Comandos de imágenes online.")]
    internal class ImageCommands : BaseCommandModule
    {
        private Config _config;
        private ImgurHelper _imgur;
        private FurryHelper _furry;

        public ImageCommands(IServiceProvider serviceProvider)
        {
            _config = (Config)serviceProvider.GetService(typeof(Config));
            _imgur = new ImgurHelper(_config.imgurClientID);
            _furry = new FurryHelper(_config._furAffinityAuth.CookieA, _config._furAffinityAuth.CookieB);
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
                throw;
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
                throw;
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
                    var ranImg = await LewdHelper.E621GetRandomImage(tag);
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
                throw;
            }
        }

        [Command("furaffinity"), Description("Obtiene una imágen de FurAffinity al azar. Es posible utilizar opciones de búsqueda.")]
        [Example("images furaffinity", "img furaffinity rating:general type:human", "img furaffinity gender:female type:anime")]
        [RequireNsfw()]
        public async Task FuraffinityCommand(CommandContext ctx, [Description("Opciones de busquedad (type, gender, rating)"), RemainingText()] Dictionary<string, string> options = null)
        {
            FurrAf_BrowseOptions searchOptions = null;
            if (options != null && options.Count != 0)
            {
                searchOptions = new FurrAf_BrowseOptions(options.ContainsKey("type")    ? options["type"]   : null,
                                                         options.ContainsKey("gender")  ? options["gender"] : null,
                                                         options.ContainsKey("rating")  ? options["rating"] : null);
            }
            int page = NumbersHelper.GetRandom(1, 100); //hardcoded af
            var imgList = await _furry.Browse(page, searchOptions);
            var img = imgList[NumbersHelper.GetRandom(0, imgList.Count)];
            var imgUrl = await _furry.GetImage(img.ID);

            var embed = new DiscordEmbedBuilder()
                .WithColor(0x3d9dd1)
                .WithTitle(img.Title)
                .WithUrl($"https://www.furaffinity.net/view/{img.ID}/")
                .WithAuthor(img.Author)
                .WithFooter($"https://www.furaffinity.net/view/{img.ID}/")
                .WithImageUrl(imgUrl);

            await ctx.RespondAsync(embed: embed);
        }
    }
}
