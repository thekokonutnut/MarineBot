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

namespace MarineBot.Commands
{
    [Group("Images"), Aliases("img")]
    [Description("Comandos de imágenes online.")]
    internal class ImageCommands : BaseCommandModule
    {
        private Config _config;
        private ImgurHelper _imgur;

        public ImageCommands(IServiceProvider serviceProvider)
        {
            _config = (Config)serviceProvider.GetService(typeof(Config));
            _imgur = new ImgurHelper(_config.imgurClientID);
        }

        [GroupCommand(), Hidden()]
        public async Task MainCommand(CommandContext ctx)
        {
            var cmds = ctx.CommandsNext;
            var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), ctx.Command.QualifiedName);
            await cmds.ExecuteCommandAsync(context);
        }

        [Command("imgur")]
        [Description("Obtiene una imágen al azar dado una o más etiquetas.")]
        public async Task ImgurCommand(CommandContext ctx, [Description("Etiqueta(s) a buscar")] string tag)
        {
            try
            {
                var ranImg = await _imgur.GetRandomImage(tag);
                if (ranImg.StartsWith("http"))
                    await ctx.RespondAsync(ranImg);
                else
                    await MessageHelper.SendWarningEmbed(ctx, ranImg);
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
                throw;
            }
        }

        [Command("safebooru")]
        [Description("Obtiene una imágen al azar dado una o más etiquetas.")]
        public async Task SafebooruCommand(CommandContext ctx, [Description("Etiqueta(s) a buscar")] string tag)
        {
            try
            {
                var ranImg = await SafebooruHelper.GetRandomImage(tag);
                if (ranImg.StartsWith("http"))
                    await ctx.RespondAsync(ranImg);
                else
                    await MessageHelper.SendWarningEmbed(ctx, ranImg);
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
                throw;
            }
        }

        [Command("e621")]
        [Description("Obtiene una imágen al azar dado una o más etiquetas.")]
        public async Task E621Command(CommandContext ctx, [Description("Etiqueta(s) a buscar")] string tag)
        {
            if (!ctx.Channel.IsNSFW)
            {
                await MessageHelper.SendWarningEmbed(ctx, "No puedes correr este comándo en un canal no NSFW.");
                return;
            }

            try
            {
                var ranImg = await LewdHelper.E621GetRandomImage(tag);
                if (ranImg.StartsWith("http"))
                    await ctx.RespondAsync(ranImg);
                else
                    await MessageHelper.SendWarningEmbed(ctx, ranImg);
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
                throw;
            }
        }

    }
}
