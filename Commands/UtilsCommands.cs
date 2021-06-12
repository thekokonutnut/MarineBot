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
    [ShortCommandsGroup]
    [RequireGuild]
    internal class UtilsCommands : BaseCommandModule
    {
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

                if (NumbersHelper.GetRandom(0, 100) > 95)
                {
                    await MessageHelper.SendWarningEmbed(ctx, "`No me decido...`");
                    return;
                }

                int chosen = NumbersHelper.GetRandom(0, options.Length - 1);
                await MessageHelper.SendSuccessEmbed(ctx, $"`Resultado:` {options[chosen]}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("duda"), Description("Aclara tus dudas.")]
        [Example("utils duda Is snessy gay?", "u duda Are you sure?")]
        public async Task DudaCommand(CommandContext ctx, [Description("Duda"), RemainingText()] string duda)
        {
            if (duda == null) throw new ArgumentException();
            try
            {
                await MessageHelper.SendSuccessEmbed(ctx, $"`{duda}:` {QuotesHelper.GetDudaAnswer()}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
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
            }
        }
    }
}