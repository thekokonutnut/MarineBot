using MarineBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;
using DSharpPlus;
using CodingSeb.ExpressionEvaluator;
using MarineBot.Attributes;

namespace MarineBot.Commands
{
    [Group("Utils"),Aliases("u")]
    [Description("Comandos de utilidad variada.")]
    [RequireGuild]
    internal class UtilsCommands : BaseCommandModule
    {
        private readonly Config _config;

        public UtilsCommands(IServiceProvider serviceProvider)
        {
            _config = (Config)serviceProvider.GetService(typeof(Config));
        }

        [GroupCommand(), Hidden()]
        public async Task MainCommand(CommandContext ctx)
        {
            var cmds = ctx.CommandsNext;
            var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), ctx.Command.QualifiedName);
            await cmds.ExecuteCommandAsync(context);
        }

        [Command("roll"), Description("Suelta un número al azar.")]
        [Example("utils roll 30", "u roll 100 60")]
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

        [Command("choose"), Description("Elige una de las opciones brindadas.")]
        [Example("utils choose Si No", "u choose \"Puede ser\" \"No creo\" \"Xd!\"")]
        public async Task ChooseCommand(CommandContext ctx, [Description("Opciones."), RemainingText()] params string[] options)
        {
            if (options == null) throw new ArgumentException();
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

        [Command("duda"), Description("Aclara tus dudas.")]
        [Example("utils duda Is snessy gay?", "u duda Are you sure?")]
        public async Task DudaCommand(CommandContext ctx, [Description("Duda"), RemainingText()] string duda)
        {
            if (duda == null) throw new ArgumentException();
            try
            {
                string[] options = _config.dudaAnswers;
                int chosen = NumbersHelper.GetRandom(0, options.Length);
                await MessageHelper.SendSuccessEmbed(ctx, $"`{duda}:` {options[chosen]}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
                throw;
            }
        }

        [Command("eval"), Description("Evalua una expresión.")]
        [Example("utils eval 2+2", "u eval 582+7934-765*21+11*400+3218")]
        public async Task EvalCommand(CommandContext ctx, [Description("Expresión"), RemainingText()] string expresion)
        {
            if (expresion == null) throw new ArgumentException();
            try
            {
                ExpressionEvaluator mEvaluator = new ExpressionEvaluator();
                await MessageHelper.SendSuccessEmbed(ctx, $"`{expresion}:` {mEvaluator.Evaluate(expresion)}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
                throw;
            }
        }

        [Command("purge"), Description("Elimina la cantidad especificada de mensajes.")]
        [Example("utils purge 10")]
        [RequireUserPermissions(Permissions.ManageMessages), RequireBotPermissions(Permissions.ManageMessages)]    
        public async Task PurgeCommand(CommandContext ctx, [Description("Cantidad de mensajes")] uint amount)
        {
            try
            {
                var messages = await ctx.Channel.GetMessagesAsync((int)amount + 1);

                await ctx.Channel.DeleteMessagesAsync(messages);
                var m = MessageHelper.SendSuccessEmbed(ctx, $"Se borraron mensajes con éxito.");
                await Task.Delay(2000);
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