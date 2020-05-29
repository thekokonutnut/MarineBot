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
    [Group("Utils"),Aliases("u")]
    [Description("Comandos de utilidad variada.")]
    public class UtilsCommands : BaseCommandModule
    {
        private Config _config;
        private ImgurHelper _imgur;

        public UtilsCommands(IServiceProvider serviceProvider)
        {
            _config = (Config)serviceProvider.GetService(typeof(Config));
            _imgur = new ImgurHelper(_config.imgurClientID);
        }

        [Command("roll")]
        [Description("Suelta un número al azar.")]
        public async Task RollCommand(CommandContext ctx, [Description("Número máximo.")] int max, [Description("Número mínimo.")] int min = 0)
        {
            try
            {
                int rand = NumbersHelper.GetRandom(min, max);
                await MessageHelper.SendSuccessEmbed(ctx, $"`Resultado:` {rand}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
                throw;
            }
        }

        [Command("choose")]
        [Description("Elige una de las opciones brindadas.")]
        public async Task ChooseCommand(CommandContext ctx, [Description("Opciones.")] params string[] options)
        {
            try
            {
                 if (options.Length < 2) {
                    await MessageHelper.SendWarningEmbed(ctx, "Preciso mínimo dos opciones.");
                    return;
                }

                int chosen = NumbersHelper.GetRandom(0, options.Length);
                await MessageHelper.SendSuccessEmbed(ctx, $"`Resultado:` {options[chosen]}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
                throw;
            }
        }

        [Command("duda")]
        [Description("Aclara tus dudas.")]
        public async Task DudaCommand(CommandContext ctx, [Description("Duda")] string duda)
        {
            try
            {
                string[] options = _config.dudaAnswers;
                int chosen = NumbersHelper.GetRandom(0, options.Length);
                await MessageHelper.SendSuccessEmbed(ctx, $"`{ctx.Message.Content.Substring(8)}:` {options[chosen]}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
                throw;
            }
        }

        [Command("eval")]
        [Description("Evalua una expresión.")]
        public async Task EvalCommand(CommandContext ctx, [Description("Expresión")] string expresion)
        {
            try
            {
                string input = ctx.Message.Content.Substring(8);
                ExpressionEvaluator mEvaluator = new ExpressionEvaluator();
                await MessageHelper.SendSuccessEmbed(ctx, $"`{input}:` {mEvaluator.Evaluate(input)}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
                throw;
            }
        }

        [Command("imgur")]
        [Description("Obtiene una imágen al azar dado una etiqueta.")]
        public async Task ImgurCommand(CommandContext ctx, [Description("Etiqueta a buscar")] string tag)
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

        [Command("purge")]
        [Description("Elimina la cantidad especificada de mensajes.")]
        [RequireUserPermissions(Permissions.Administrator)]
        [RequireBotPermissions(Permissions.ManageMessages)]    
        public async Task PurgeCommand(CommandContext ctx, [Description("Cantidad de mensajes")] uint amount)
        {
            try
            {
                var messages = await ctx.Channel.GetMessagesAsync((int)amount + 1);

                await ctx.Channel.DeleteMessagesAsync(messages);
                const int delay = 2000;
                var m = MessageHelper.SendSuccessEmbed(ctx, $"Se borraron mensajes con éxito.");
                await Task.Delay(delay);
                await m.Result.DeleteAsync();
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
                throw;
            }
        }
    }
}